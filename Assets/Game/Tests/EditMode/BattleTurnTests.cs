using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Events;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Turns inside a battle: who acts first, «Готов», ability limits.</summary>
    public class BattleTurnTests
    {
        [Test]
        public void BattleOnGuestTurn_HostPlacedEarlier_HostActsFirst()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            var started = d.Last<BattleStarted>();
            Assert.AreEqual(P1, started.Host);
            Assert.AreEqual(P2, started.Guest);
            Assert.AreEqual(P1, started.FirstActor);
        }

        [Test]
        public void BattleOnHostTurn_GuestPlacedEarlier_GuestActsFirst()
        {
            var d = Begin(P2);
            d.Place(P2, Water200, P1, Forge);
            d.EndTurn(P2);
            d.Place(P1, Fire180, P1, Forge);

            var started = d.Last<BattleStarted>();
            Assert.AreEqual(P1, started.Host);
            Assert.AreEqual(P2, started.FirstActor);
        }

        [Test]
        public void OneReady_DoesNotResolve_TurnPasses()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);

            Assert.IsNotNull(d.State.Battle);
            Assert.IsTrue(d.State.Battle.IsReady(P1));
            Assert.AreEqual(P2, d.State.Battle.CurrentActor);
            Assert.IsFalse(d.Log.OfType<BattleResolved>().Any());
        }

        [Test]
        public void BothReadyInARow_ResolvesBattle()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);
            d.Ready(P2);

            Assert.IsNull(d.State.Battle);
            Assert.AreEqual(1, d.Log.OfType<BattleResolved>().Count());
        }

        [Test]
        public void AbilityAfterOpponentReady_ResetsReadiness_AndReturnsTurn()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Ready(P1);
            d.Play(P2, Boost50);

            Assert.IsFalse(d.State.Battle.IsReady(P1));
            Assert.AreEqual(P1, d.Last<ReadinessReset>().Player);
            Assert.AreEqual(P1, d.State.Battle.CurrentActor);

            d.Ready(P1);
            Assert.IsNotNull(d.State.Battle, "P2 has not pressed Ready, so the battle goes on.");
            Assert.AreEqual(P2, d.State.Battle.CurrentActor);
        }

        [Test]
        public void PlayingAbility_PassesTurn_AndSpendsCard()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            d.Play(P1, Boost50);

            Assert.AreEqual(P2, d.State.Battle.CurrentActor);
            Assert.IsTrue(d.InDiscard(P1, Boost50));
            Assert.AreEqual(Boost50, d.Last<AbilityPlayed>().Ability.CardId);
        }

        [Test]
        public void AbilityOutOfTurn_Rejected()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            d.Rejected(new PlayAbility(P2, d.Id(P2, Boost50)), RejectReason.NotYourTurn);
        }

        [Test]
        public void ReadyOutOfTurn_Rejected()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            d.Rejected(new Ready(P2), RejectReason.NotYourTurn);
        }

        [Test]
        public void AbilityOutsideBattle_Rejected()
        {
            var d = Begin(P1);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Boost50)), RejectReason.TimingNotAllowed);
        }

        [Test]
        public void CreatureCannotBePlayedAsAbility()
        {
            var d = BattleOnP1Forge(Fire180, Water200);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Earth250)), RejectReason.NotAnAbility);
        }

        [Test]
        public void SecondLegendaryInOneTurn_Rejected()
        {
            var rules = RulesWithDeck(
                fields: new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary },
                creatures: new[] { Rarity.Common, Rarity.Epic, Rarity.Legendary },
                abilities: new[] { Rarity.Common, Rarity.Legendary, Rarity.Legendary });
            var deck = new Decks.DeckList(new[]
            {
                Plain, Forge, Temple, Fire180, Water200, Earth250, Boost50, Gambit, Gambit2,
            });
            var d = BattleOnP1Forge(Fire180, Water200, rules, deck, deck);
            d.Play(P1, Gambit);
            d.Ready(P2);

            d.Rejected(new PlayAbility(P1, d.Id(P1, Gambit2)), RejectReason.LegendaryLimitReached);
        }

        [Test]
        public void SignatureAbility_WithItsCreatureInBattle_AddsPower()
        {
            var deck = Deck(legendaryAbility: EarthSignature);
            var d = BattleOnP1Forge(Earth250, Fire180, player1Deck: deck);
            d.Play(P1, EarthSignature);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(250 + 60, d.Last<BattleResolved>().HostPower);
        }

        [Test]
        public void SignatureAbility_WithAnotherCreatureInBattle_Rejected()
        {
            var deck = Deck(legendaryAbility: EarthSignature);
            var d = BattleOnP1Forge(Fire180, Water200, player1Deck: deck);

            d.Rejected(new PlayAbility(P1, d.Id(P1, EarthSignature)), RejectReason.SignatureCreatureNotInBattle);
        }
    }
}
