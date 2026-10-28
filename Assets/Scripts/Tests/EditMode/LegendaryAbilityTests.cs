using System.Linq;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Legendary abilities, their play windows and signature abilities (docs/cards.md §4–5).</summary>
    public class LegendaryAbilityTests
    {
        [Test]
        public void Maneuver_BeforeMainAction_MovesOwnCreatureAndStartsBattle()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Maneuver));
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Plain);
            d.EndTurn(P2);

            var target = d.Field(P2, Plain);
            d.Ok(new PlayAbility(P1, d.Id(P1, Maneuver), new AbilityTargets(creatureId: d.Id(P1, Fire180), fieldId: target.Id)));

            Assert.IsNull(d.Field(P1, Plain).OccupantOf(P1));
            var started = d.Last<BattleStarted>();
            Assert.AreEqual(target.Id, started.FieldId);
            Assert.AreEqual(P2, started.FirstActor, "The moved creature counts as placed later.");

            d.Ready(P2);
            d.Ready(P1);
            d.Rejected(new EndTurn(P1), RejectReason.MainActionRequired);
        }

        [Test]
        public void Maneuver_AfterMainAction_Rejected()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Maneuver));
            d.Place(P1, Fire180, P1, Plain);

            var targets = new AbilityTargets(creatureId: d.Id(P1, Fire180), fieldId: d.Field(P2, Plain).Id);
            d.Rejected(new PlayAbility(P1, d.Id(P1, Maneuver), targets), RejectReason.TimingNotAllowed);
        }

        [Test]
        public void Maneuver_ToFieldWithOwnCreature_Rejected()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Maneuver));
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Plain);
            d.EndTurn(P2);

            var targets = new AbilityTargets(creatureId: d.Id(P1, Fire180), fieldId: d.Field(P1, Plain).Id);
            d.Rejected(new PlayAbility(P1, d.Id(P1, Maneuver), targets), RejectReason.InvalidTarget);
        }

        [Test]
        public void Displacement_MovesOpponentCreatureOntoYours()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Displacement));
            d.Place(P1, Fire180, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Plain);
            d.EndTurn(P2);

            var target = d.Field(P1, Plain);
            d.Ok(new PlayAbility(P1, d.Id(P1, Displacement), new AbilityTargets(creatureId: d.Id(P2, Water200), fieldId: target.Id)));

            var started = d.Last<BattleStarted>();
            Assert.AreEqual(target.Id, started.FieldId);
            Assert.AreEqual(P1, started.Host);
            Assert.AreEqual(P1, started.FirstActor);
        }

        [Test]
        public void Retreat_OnOpponentTurn_EndsBattleWithoutWinner()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(legendaryAbility: Retreat));
            Assert.AreEqual(P1, d.State.Battle.CurrentActor);

            d.Play(P2, Retreat);

            Assert.IsNull(d.State.Battle);
            Assert.IsTrue(d.Log.OfType<BattleRetreated>().Any());
            Assert.IsFalse(d.Log.OfType<BattleResolved>().Any());
            Assert.IsTrue(d.InHand(P2, Water200));
            Assert.AreEqual(Fire180, d.Field(P1, Forge).OccupantOf(P1).Definition.Id);
            Assert.IsFalse(d.Field(P1, Forge).IsRemoved);
            Assert.AreEqual(0, d.Wins(P1) + d.Wins(P2));
            Assert.AreEqual(P2, d.State.ActivePlayer);
        }

        [Test]
        public void Retreat_FromOpenField_RemovesIt()
        {
            var d = BattleOnP1Forge(Fire180, Water200, player2Deck: Deck(legendaryAbility: Retreat));
            d.Open(P1);
            d.Ready(P1);
            d.Play(P2, Retreat);

            Assert.IsTrue(d.Field(P1, Forge).IsRemoved);
            Assert.IsTrue(d.InHand(P2, Water200));
            Assert.IsTrue(d.InHand(P1, Fire180));
        }

        [Test]
        public void Retreat_FromUnbreakableField_FieldAndOpponentCreatureStay()
        {
            var d = BattleOnP1Field(UnbreakableField, Fire180, Water200,
                player1Deck: Deck(legendaryField: UnbreakableField), player2Deck: Deck(legendaryAbility: Retreat));
            d.Open(P1);
            d.Ready(P1);
            d.Play(P2, Retreat);

            var field = d.Field(P1, UnbreakableField);
            Assert.IsFalse(field.IsRemoved);
            Assert.IsTrue(field.IsOpen);
            Assert.AreEqual(Fire180, field.OccupantOf(P1).Definition.Id);
            Assert.IsTrue(d.InHand(P2, Water200));
        }

        [Test]
        public void Retreat_FromUnbreakableField_WithCancelledCondition_RemovesIt()
        {
            var d = BattleOnP1Field(UnbreakableField, Fire180, Water200,
                player1Deck: Deck(legendaryField: UnbreakableField),
                player2Deck: Deck(epicAbility: CancelCondition, legendaryAbility: Retreat));
            d.Open(P1);
            d.Ready(P1);
            d.Play(P2, CancelCondition);
            d.Ready(P1);
            d.Play(P2, Retreat);

            Assert.IsTrue(d.Field(P1, UnbreakableField).IsRemoved);
            Assert.IsTrue(d.InHand(P1, Fire180));
        }

        [Test]
        public void CommonAbility_OnOpponentTurn_Rejected()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            d.Rejected(new PlayAbility(P2, d.Id(P2, Boost50)), RejectReason.NotYourTurn);
        }

        [Test]
        public void SecondLife_AfterBattle_ReturnsCreatureFromOutOfGame()
        {
            var d = BattleOnP1ForgeInP1Turn(Fire180, Water200, Deck(legendaryAbility: SecondLife));
            d.Ready(P2);
            d.Ready(P1);
            Assert.IsTrue(d.IsOut(P1, Fire180));

            d.Ok(new PlayAbility(P1, d.Id(P1, SecondLife), new AbilityTargets(cardId: d.Id(P1, Fire180))));
            Assert.IsTrue(d.InHand(P1, Fire180));
        }

        [Test]
        public void SecondLife_WithoutBattleThisTurn_Rejected()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: SecondLife));
            d.Place(P1, Fire180, P1, Plain);

            d.Rejected(new PlayAbility(P1, d.Id(P1, SecondLife), new AbilityTargets(cardId: d.Id(P1, Fire180))),
                RejectReason.TimingNotAllowed);
        }

        [Test]
        public void ReturnPower_AfterBattle_ReturnsAbilityFromDiscard()
        {
            var d = BattleOnP1ForgeInP1Turn(Fire180, Water200, Deck(legendaryAbility: ReturnPower));
            d.Ready(P2);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);
            Assert.IsTrue(d.InDiscard(P1, Boost50));

            d.Ok(new PlayAbility(P1, d.Id(P1, ReturnPower), new AbilityTargets(cardId: d.Id(P1, Boost50))));
            Assert.IsTrue(d.InHand(P1, Boost50));
            Assert.AreEqual(Boost50, d.Last<AbilityReturnedToHand>().Ability.CardId);
        }

        [Test]
        public void Desolation_OpponentDiscardsRandomAbility()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Desolation));
            d.Play(P1, Desolation);

            Assert.AreEqual(1, d.State.Player(P2).Discard.Count);
            Assert.AreEqual(P2, d.Last<AbilityDiscarded>().Owner);
        }

        [Test]
        public void Scout_RevealsOpponentAbilitiesAndField_ToCasterOnly()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Scout));
            d.Place(P1, Fire180, P1, Plain);
            d.Ok(new PlayAbility(P1, d.Id(P1, Scout), new AbilityTargets(fieldId: d.Field(P2, Forge).Id)));

            var scouted = d.Last<Scouted>();
            Assert.AreEqual(3, scouted.OpponentAbilities.Count);
            Assert.AreEqual(Forge, scouted.Field.CardId);
            Assert.IsTrue(scouted.IsVisibleTo(P1));
            Assert.IsFalse(scouted.IsVisibleTo(P2));
        }

        [Test]
        public void Scout_WithoutFieldTarget_Rejected()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: Scout));

            d.Rejected(new PlayAbility(P1, d.Id(P1, Scout)), RejectReason.InvalidTarget);
        }

        [Test]
        public void LockedField_HostCannotOpenInNextBattle()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: LockField));
            var forge = d.Field(P2, Forge);
            d.Ok(new PlayAbility(P1, d.Id(P1, LockField), new AbilityTargets(fieldId: forge.Id)));
            d.Place(P1, Fire180, P2, Forge);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Forge);
            d.Ready(P1);

            d.Rejected(new OpenField(P2), RejectReason.FieldLocked);

            d.Ready(P2);
            Assert.IsFalse(forge.IsLocked, "The lock lasts for one battle.");
        }

        [Test]
        public void LockedField_ForcedRevealDoesNotOpenIt()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: LockField), player2Deck: Deck(epicAbility: ForcedReveal));
            var forge = d.Field(P2, Forge);
            d.Ok(new PlayAbility(P1, d.Id(P1, LockField), new AbilityTargets(fieldId: forge.Id)));
            d.Place(P1, Fire180, P2, Forge);
            d.EndTurn(P1);
            d.Place(P2, Water200, P2, Forge);
            d.Ready(P1);
            d.Play(P2, ForcedReveal);

            Assert.IsFalse(forge.IsOpen);
            Assert.IsFalse(d.Log.OfType<FieldOpened>().Any());
        }

        // ---- «Рассвет» ----

        [Test]
        public void Dawn_FieldPenaltyDoesNotActOnCreature()
        {
            var d = BattleOnP1Field(Quarry, Earth250, Fire180,
                player1Deck: Deck(legendaryAbility: Dawn, commonField: Quarry));
            d.Open(P1);
            d.Play(P1, Dawn);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(250 + 70, d.Last<BattleResolved>().HostPower, "Earth −40 is ignored.");
            Assert.AreEqual(180 + 40, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void Dawn_PenaltyTurnedIntoBonus_StillActs()
        {
            var d = BattleOnP1Field(Quarry, Earth250, Fire180,
                player1Deck: Deck(legendaryAbility: Dawn, commonField: Quarry),
                player2Deck: Deck(epicAbility: Mirror));
            d.Open(P1);
            d.Play(P1, Dawn);
            d.Play(P2, Mirror);
            d.Ready(P1);
            d.Ready(P2);

            Assert.AreEqual(250 + 70 + 40, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(180 - 40, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void Dawn_FieldConditionStillActs()
        {
            var d = BattleOnP1Field(SacrificeField, Earth250, Fire180,
                player1Deck: Deck(legendaryAbility: Dawn, legendaryField: SacrificeField));
            d.Open(P1);
            d.Play(P1, Dawn);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(P1, d.Last<BattleResolved>().Winner);
            Assert.IsTrue(d.IsOut(P1, Earth250), "«Жертва» acts on the winner with «Рассвет».");
        }

        [Test]
        public void LegendaryLimit_AppliesOutsideBattleToo()
        {
            var rules = RulesWithDeck(
                fields: new[] { Cards.Rarity.Common, Cards.Rarity.Epic, Cards.Rarity.Legendary },
                creatures: new[] { Cards.Rarity.Common, Cards.Rarity.Epic, Cards.Rarity.Legendary },
                abilities: new[] { Cards.Rarity.Common, Cards.Rarity.Legendary, Cards.Rarity.Legendary });
            var deck = new Decks.DeckList(new[] { Plain, Forge, Temple, Fire180, Water200, Earth250, Boost50, Desolation, Scout });
            var d = Begin(P1, rules, deck, deck);
            d.Play(P1, Desolation);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Scout), new AbilityTargets(fieldId: d.Field(P2, Forge).Id)),
                RejectReason.LegendaryLimitReached);
        }

        // ---- Signature abilities ----

        [Test]
        public void SignatureWithPlacement_PlayedBeforeBattle_TakesFirstTurn()
        {
            var d = Begin(P1, player2Deck: Deck(legendaryAbility: StormRush));
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);
            d.Ok(new PlaceCreature(P2, d.Id(P2, Earth250), d.Field(P1, Forge).Id, signatureAbilityId: d.Id(P2, StormRush)));

            var abilityIndex = d.Log.ToList().FindLastIndex(e => e is AbilityPlayed);
            var battleIndex = d.Log.ToList().FindLastIndex(e => e is BattleStarted);
            Assert.Less(abilityIndex, battleIndex, "The signature ability is played before the battle starts.");
            Assert.AreEqual(P2, d.Last<BattleStarted>().FirstActor);

            d.Ready(P2);
            d.Ready(P1);
            Assert.AreEqual(250 + 70, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void SignatureWithPlacement_WithoutBattle_Rejected()
        {
            var d = Begin(P1, player1Deck: Deck(legendaryAbility: StormRush));

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Earth250), d.Field(P1, Plain).Id, signatureAbilityId: d.Id(P1, StormRush)),
                RejectReason.SignatureNeedsBattle);
        }

        [Test]
        public void SignatureWithPlacement_OtherCreature_Rejected()
        {
            var d = Begin(P1, player2Deck: Deck(legendaryAbility: StormRush));
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);

            d.Rejected(new PlaceCreature(P2, d.Id(P2, Water200), d.Field(P1, Forge).Id, signatureAbilityId: d.Id(P2, StormRush)),
                RejectReason.SignatureCreatureNotInBattle);
        }

        [Test]
        public void BeforeBattleOnlySignature_InBattle_Rejected()
        {
            var d = BattleOnP1Forge(Earth250, Fire180, player1Deck: Deck(legendaryAbility: StormRush));

            d.Rejected(new PlayAbility(P1, d.Id(P1, StormRush)), RejectReason.TimingNotAllowed);
        }

        [Test]
        public void ForgeFlame_AddsOwnPower_AndLowersOpponents()
        {
            var d = BattleOnP1Forge(Earth250, Fire180, player1Deck: Deck(legendaryAbility: ForgeFlame));
            d.Play(P1, ForgeFlame);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(250 + 80, d.Last<BattleResolved>().HostPower);
            Assert.AreEqual(180 - 20, d.Last<BattleResolved>().GuestPower);
        }

        [Test]
        public void DarkPrice_RequiresAbilityToDiscard()
        {
            var d = BattleOnP1Forge(Earth250, Water200, player1Deck: Deck(legendaryAbility: DarkPrice));

            d.Rejected(new PlayAbility(P1, d.Id(P1, DarkPrice)), RejectReason.InvalidTarget);

            d.Ok(new PlayAbility(P1, d.Id(P1, DarkPrice), new AbilityTargets(cardId: d.Id(P1, Boost50))));
            Assert.IsTrue(d.InDiscard(P1, Boost50));
            Assert.AreEqual(Boost50, d.Last<AbilityDiscarded>().Ability.CardId);

            d.Ready(P2);
            d.Ready(P1);
            Assert.AreEqual(250 + 100, d.Last<BattleResolved>().HostPower);
        }

        /// <summary>Battle on Player1's Forge in Player1's own turn: Player2's creature came first, so Player2 acts first.</summary>
        private static MatchDriver BattleOnP1ForgeInP1Turn(string hostCreature, string guestCreature, Decks.DeckList player1Deck)
        {
            var d = Begin(P2, player1Deck: player1Deck);
            d.Place(P2, guestCreature, P1, Forge);
            d.EndTurn(P2);
            d.Place(P1, hostCreature, P1, Forge);
            return d;
        }
    }
}
