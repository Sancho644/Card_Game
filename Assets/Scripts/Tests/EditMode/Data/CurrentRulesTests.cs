using System.Linq;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Randomness;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using NUnit.Framework;
using UnityEditor;

namespace Game.Data.Tests
{
    /// <summary>Runs the approved rules against the actual card assets, without balance simulations.</summary>
    public sealed class CurrentRulesTests
    {
        private const string Altar = "field.sacrificial-altar";
        private const string Strong = "creature.prince-of-abyss";
        private const string Weak = "creature.stone-beetle";
        private const string Epic = "creature.whirlwind-griffin";
        private const string BreakCondition = "ability.break-condition";
        private CardDatabase _database;

        [SetUp]
        public void SetUp() => _database = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");

        [TestCase(PlayerId.Player1, true, true, false)]
        [TestCase(PlayerId.Player2, true, true, false)]
        [TestCase(PlayerId.Player1, false, true, false)]
        [TestCase(PlayerId.Player2, false, true, false)]
        [TestCase(PlayerId.Player1, true, false, false)]
        [TestCase(PlayerId.Player2, true, false, false)]
        [TestCase(PlayerId.Player1, false, false, false)]
        [TestCase(PlayerId.Player2, false, false, false)]
        [TestCase(PlayerId.Player1, true, true, true)]
        [TestCase(PlayerId.Player2, true, true, true)]
        public void Altar_OnlyActiveConditionKeepsWinningGuest(PlayerId host, bool open, bool guestWins, bool cancel)
        {
            var guest = host.Opponent();
            var engine = Begin(host);
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            StartBattle(engine, field, guestWins ? Weak : Strong, guestWins ? Strong : Weak);
            if (open) Ok(engine, new OpenField(host));
            if (cancel)
            {
                Ok(engine, new PlayAbility(host, Id(engine, host, BreakCondition)));
                Ok(engine, new Ready(guest));
            }
            Ok(engine, new Ready(host));
            if (!cancel) Ok(engine, new Ready(guest));
            Assert.IsNull(engine.State.Battle);
            var winner = guestWins ? guest : host;
            Assert.AreEqual(1, engine.State.Player(winner).Wins);
            var holds = open && guestWins && !cancel;
            Assert.AreEqual(holds, field.OccupantOf(guest) != null);
            Assert.IsNull(field.OccupantOf(host));
            Assert.AreEqual(!holds && (open || guestWins), field.IsRemoved);
            Assert.AreEqual(holds ? 0 : 1, engine.State.Player(guest).OutOfGame.Count);
            Assert.AreEqual(1, engine.State.Player(host).OutOfGame.Count);
            if (holds) Assert.IsTrue(field.IsOpen);
        }

        [Test]
        public void WinningGuest_BlocksCycleAndCanBeAttackedAgain()
        {
            var host = PlayerId.Player1;
            var guest = PlayerId.Player2;
            var engine = Begin(host);
            // Arrange a spent group: this guest has only its last creature left to play.
            var state = engine.State.Player(guest);
            foreach (var creature in state.Hand.Where(c => c.Type == CardType.Creature && c.Definition.Id != Strong).ToArray())
            {
                typeof(PlayerState).GetMethod("RemoveFromHand", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(state, new object[] { creature });
                typeof(PlayerState).GetMethod("AddToOutOfGame", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(state, new object[] { creature });
            }
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            StartBattle(engine, field, Weak, Strong);
            Ok(engine, new OpenField(host));
            Ok(engine, new Ready(host));
            Ok(engine, new Ready(guest));
            Assert.IsFalse(state.HasCreatureInHand);
            Assert.AreEqual(2, state.OutOfGame.Count, "A surviving creature on the table prevents a group return.");
            Assert.AreEqual(Strong, field.OccupantOf(guest).Definition.Id);
            Ok(engine, new EndTurn(guest));
            Ok(engine, new PlaceCreature(host, Id(engine, host, Epic), field.Id));
            Assert.AreEqual(guest, engine.State.Battle.CurrentActor, "The surviving guest was placed earlier.");
        }

        [Test]
        public void Altar_RetreatDoesNotTriggerWinningGuestCondition()
        {
            var host = PlayerId.Player1;
            var guest = PlayerId.Player2;
            var engine = Begin(host);
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            StartBattle(engine, field, Weak, Strong);
            Ok(engine, new OpenField(host));
            Ok(engine, new PlayAbility(guest, Id(engine, guest, "ability.retreat")));
            Assert.IsTrue(field.IsRemoved);
            Assert.IsNull(field.OccupantOf(host));
            Assert.IsNull(field.OccupantOf(guest));
            Assert.AreEqual(0, engine.State.Player(guest).Wins);
        }

        [TestCase("ability.forge-flame", 80, -20)]
        [TestCase("ability.ebb", 60, 0)]
        [TestCase("ability.stone-will", 60, 0)]
        [TestCase("ability.storm-onslaught", 50, 0)]
        [TestCase("ability.dawn", 70, 0)]
        [TestCase("ability.dark-price", 100, 0)]
        public void Signature_OnlyInBattleForOwnCreature_AndPassesTurn(string abilityId, int bonus, int opponentBonus)
        {
            var owner = PlayerId.Player1;
            var opponent = PlayerId.Player2;
            var ability = (AbilityDef)_database.Find(abilityId);
            var creatureId = ability.BoundCreature.Id;
            var engine = Begin(owner, abilityId);
            var field = engine.State.Fields.Single(f => f.Owner == owner && f.Definition.Id == Altar);
            var signatureId = Id(engine, owner, abilityId);
            var targets = abilityId == "ability.dark-price" ? new AbilityTargets(cardId: Id(engine, owner, "ability.heat")) : AbilityTargets.None;
            Assert.AreEqual(RejectReason.TimingNotAllowed, engine.Execute(new PlayAbility(owner, signatureId, targets)).Reason);
            Ok(engine, new PlaceCreature(owner, Id(engine, owner, creatureId), field.Id));
            Ok(engine, new EndTurn(owner));
            Ok(engine, new PlaceCreature(opponent, Id(engine, opponent, Weak), field.Id));
            var before = engine.GetBattlePower();
            Ok(engine, new PlayAbility(owner, signatureId, targets));
            var after = engine.GetBattlePower();
            Assert.AreEqual(before.HostPower.FinalPower + bonus, after.HostPower.FinalPower);
            Assert.AreEqual(before.GuestPower.FinalPower + opponentBonus, after.GuestPower.FinalPower);
            Assert.AreEqual(opponent, engine.State.Battle.CurrentActor);
            Assert.IsTrue(engine.State.Player(owner).Discard.Any(c => c.Id == signatureId));
            Ok(engine, new Ready(opponent));
            Ok(engine, new Ready(owner));
            Assert.IsNull(engine.State.Battle);
        }

        [Test]
        public void Signature_ForOtherCreatureAndOnOpponentsBattleTurn_IsRejected()
        {
            var host = PlayerId.Player1;
            var guest = PlayerId.Player2;
            var engine = Begin(host, "ability.storm-onslaught");
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            StartBattle(engine, field, Weak, Weak);
            var signatureId = Id(engine, host, "ability.storm-onslaught");
            Assert.AreEqual(RejectReason.SignatureCreatureNotInBattle,
                engine.Execute(new PlayAbility(host, signatureId)).Reason);
            Ok(engine, new Ready(host));
            Assert.AreEqual(guest, engine.State.Battle.CurrentActor);
            Assert.AreEqual(RejectReason.NotYourTurn, engine.Execute(new PlayAbility(host, signatureId)).Reason);
            Assert.IsTrue(engine.State.Player(host).Hand.Any(c => c.Id == signatureId));
            Assert.IsTrue(engine.State.Battle.IsReady(host));
        }

        [Test]
        public void StormOnslaught_GuestCanPlayDuringOpponentsMatchTurn()
        {
            var host = PlayerId.Player2;
            var guest = PlayerId.Player1;
            var engine = Begin(guest, "ability.storm-onslaught");
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            Ok(engine, new PlaceCreature(guest, Id(engine, guest, "creature.hurricane-king"), field.Id));
            Ok(engine, new EndTurn(guest));
            Ok(engine, new PlaceCreature(host, Id(engine, host, Weak), field.Id));
            Assert.AreEqual(host, engine.State.ActivePlayer);
            var before = engine.GetBattlePower().GuestPower.FinalPower;
            Ok(engine, new PlayAbility(guest, Id(engine, guest, "ability.storm-onslaught")));
            Assert.AreEqual(before + 50, engine.GetBattlePower().GuestPower.FinalPower);
            Assert.AreEqual(host, engine.State.Battle.CurrentActor);
            Assert.IsFalse(engine.State.Battle.IsReady(host));
        }

        [Test]
        public void UnplayedSignature_IsRejectedAfterBattle()
        {
            var host = PlayerId.Player1;
            var guest = PlayerId.Player2;
            var engine = Begin(host, "ability.storm-onslaught");
            var field = engine.State.Fields.Single(f => f.Owner == host && f.Definition.Id == Altar);
            StartBattle(engine, field, "creature.hurricane-king", Weak);
            Ok(engine, new Ready(host));
            Ok(engine, new Ready(guest));
            Ok(engine, new EndTurn(guest));
            Assert.AreEqual(RejectReason.TimingNotAllowed,
                engine.Execute(new PlayAbility(host, Id(engine, host, "ability.storm-onslaught"))).Reason);
        }

        private MatchEngine Begin(PlayerId first, string signatureId = null)
        {
            var engine = new MatchEngine(new MatchSetup(Deck(signatureId), Deck()), _database.ToCatalog(),
                AssetDatabase.LoadAssetAtPath<GameRulesConfig>("Assets/Data/Config/GameRules.asset").ToRules(), new XorShiftRandom(1));
            engine.Start();
            foreach (var player in PlayerIds.All)
                Ok(engine, new ChooseCard(player, engine.State.Fields.Single(f => f.Owner == player && f.Definition.Id == Altar).Id));
            var coinWinner = engine.State.CoinTossWinner.Value;
            Ok(engine, new ChooseTurnOrder(coinWinner, coinWinner == first));
            return engine;
        }

        private DeckList Deck(string signatureId = null) => new DeckList(new[]
        {
            "field.old-altar", "field.forge", Altar,
            Weak, Epic, signatureId == null ? Strong : ((AbilityDef)_database.Find(signatureId)).BoundCreature.Id,
            "ability.heat", BreakCondition, signatureId ?? "ability.retreat",
        });

        private static void StartBattle(MatchEngine engine, FieldSlot field, string hostCreature, string guestCreature)
        {
            Ok(engine, new PlaceCreature(field.Owner, Id(engine, field.Owner, hostCreature), field.Id));
            Ok(engine, new EndTurn(field.Owner));
            Ok(engine, new PlaceCreature(field.Owner.Opponent(), Id(engine, field.Owner.Opponent(), guestCreature), field.Id));
        }

        private static int Id(MatchEngine engine, PlayerId owner, string cardId) =>
            engine.State.Player(owner).Hand.Single(c => c.Definition.Id == cardId).Id;

        private static void Ok(MatchEngine engine, GameCommand command)
        {
            var result = engine.Execute(command);
            Assert.IsTrue(result.IsAccepted, command.GetType().Name + ": " + result.Reason);
        }
    }
}
