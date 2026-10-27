using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Decks;
using Game.Core.Events;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>Coin toss, turn order, end of match, determinism, hidden information.</summary>
    public class MatchFlowTests
    {
        [Test]
        public void CoinToss_SameSeed_SameWinner()
        {
            Assert.AreEqual(Create(seed: 7).State.CoinTossWinner, Create(seed: 7).State.CoinTossWinner);
        }

        [Test]
        public void CoinToss_BothPlayersCanWin()
        {
            var winners = Enumerable.Range(1, 32)
                .Select(seed => Create(seed: (ulong)seed).State.CoinTossWinner)
                .Distinct()
                .Count();

            Assert.AreEqual(2, winners);
        }

        [Test]
        public void OnlyCoinTossWinner_ChoosesTurnOrder()
        {
            var d = Create();
            var loser = d.State.CoinTossWinner.Value == P1 ? P2 : P1;

            d.Rejected(new ChooseTurnOrder(loser, goFirst: true), RejectReason.NotCoinTossWinner);
        }

        [Test]
        public void CoinTossWinner_CanChooseToMoveSecond()
        {
            var d = Create();
            var winner = d.State.CoinTossWinner.Value;
            d.Ok(new ChooseTurnOrder(winner, goFirst: false));

            Assert.AreNotEqual(winner, d.State.ActivePlayer);
            Assert.AreEqual(d.State.ActivePlayer, d.Last<TurnStarted>().Player);
        }

        [Test]
        public void Commands_BeforeTurnOrderIsChosen_Rejected()
        {
            var d = Create();

            d.Rejected(new PlaceCreature(P1, d.Id(P1, Fire180), d.Field(P1, Plain).Id), RejectReason.WrongPhase);
        }

        [Test]
        public void ThirdWin_EndsMatchImmediately()
        {
            var d = Begin(P1);
            d.Place(P1, Earth250, P1, Plain);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P1, Plain);
            d.Ready(P1);
            d.Ready(P2);
            d.EndTurn(P2);

            d.Place(P1, Fire180, P1, Temple);
            d.EndTurn(P1);
            d.Place(P2, Water200, P1, Plain);
            d.Ready(P1);
            d.Ready(P2);
            d.EndTurn(P2);

            d.Place(P1, Water200, P1, Forge);
            d.EndTurn(P1);
            d.Place(P2, Earth250, P1, Plain);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(3, d.Wins(P1));
            Assert.AreEqual(MatchPhase.Ended, d.State.Phase);
            Assert.AreEqual(P1, d.Last<MatchEnded>().Winner);
            Assert.AreEqual(P1, d.State.Result.Winner);
        }

        [Test]
        public void Commands_AfterMatchEnd_Rejected()
        {
            var d = Begin(P1, SmallRules(winsToWin: 1), SmallDeck(), SmallDeck());
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P1, Forge);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);

            Assert.AreEqual(P1, d.Last<MatchEnded>().Winner);
            d.Rejected(new EndTurn(P2), RejectReason.MatchOver);
        }

        [Test]
        public void BothSkipInARow_PlayerWithMoreWinsWins()
        {
            var d = Begin(P1, SmallRules(), SmallDeck(), SmallDeck());
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P1, Forge);
            d.Play(P1, Boost50);
            d.Ready(P2);
            d.Ready(P1);
            d.EndTurn(P2);

            // Player1's creature holds the Forge; Player2 has no creatures left: no main action for anyone.
            d.EndTurn(P1);
            d.EndTurn(P2);

            var skips = d.Log.OfType<MainActionSkipped>().ToList();
            Assert.AreEqual(2, skips.Count);
            Assert.AreEqual(SkipReason.NoChallengeTarget, skips[0].Reason, "Player1 holds the only field and has nothing to challenge.");
            Assert.AreEqual(SkipReason.NoCreatures, skips[1].Reason, "Player2 has no creatures anywhere.");
            Assert.AreEqual(P1, d.Last<MatchEnded>().Winner);
        }

        [Test]
        public void BothSkipInARow_EqualWins_Draw()
        {
            var d = Begin(P1, SmallRules(), SmallDeck(), SmallDeck());
            d.Place(P1, Fire180, P1, Forge);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P1, Forge);
            d.Open(P1);
            d.Ready(P1);
            d.Ready(P2);
            d.EndTurn(P2);

            d.Place(P1, Fire180, P2, Forge);
            d.EndTurn(P1);
            d.Place(P2, Fire180, P2, Forge);
            d.Ready(P1);
            d.Open(P2);
            d.Ready(P2);
            d.Ready(P1);
            d.EndTurn(P2);

            // No field cards left.
            d.EndTurn(P1);
            d.EndTurn(P2);

            Assert.AreEqual(0, d.Wins(P1));
            Assert.AreEqual(0, d.Wins(P2));
            Assert.IsTrue(d.State.Result.IsDraw);
            Assert.IsNull(d.Last<MatchEnded>().Winner);
        }

        [Test]
        public void SameSeedAndCommands_ProduceSameEvents()
        {
            Assert.AreEqual(Describe(PlayScript(42)), Describe(PlayScript(42)));
        }

        [Test]
        public void HandDealt_IsVisibleOnlyToOwner()
        {
            var d = Create();
            var hand = d.Log.OfType<HandDealt>().First(e => e.Owner == P1);

            Assert.IsTrue(hand.IsVisibleTo(P1));
            Assert.IsFalse(hand.IsVisibleTo(P2));
        }

        [Test]
        public void PlayerView_HidesOpponentHandAndClosedFields()
        {
            var d = Begin(P1);
            var view = d.Engine.GetView(P1);

            Assert.AreEqual(6, view.Hand.Count);
            Assert.AreEqual(6, view.OpponentHandCount);
            foreach (var field in view.Fields)
            {
                if (field.Owner == P1)
                    Assert.IsNotNull(field.CardId);
                else
                    Assert.IsNull(field.CardId);
            }
        }

        [Test]
        public void PlayerView_ShowsFieldOnceOpened_AndCreaturesAlways()
        {
            var d = BattleOnP1Forge(Fire180, Water200);
            var forgeId = d.Field(P1, Forge).Id;

            var before = d.Engine.GetView(P2).Field(forgeId);
            Assert.IsNull(before.CardId);
            Assert.AreEqual(Fire180, before.Occupant(P1).CardId);

            d.Open(P1);
            Assert.AreEqual(Forge, d.Engine.GetView(P2).Field(forgeId).CardId);
        }

        /// <summary>One card per type: Forge, Fire180, Boost50.</summary>
        private static DeckList SmallDeck() => new DeckList(new[] { Forge, Fire180, Boost50 });

        private static GameRules SmallRules(int winsToWin = 3) =>
            RulesWithDeck(new[] { Rarity.Epic }, new[] { Rarity.Common }, new[] { Rarity.Common }, winsToWin);

        private static IReadOnlyList<GameEvent> PlayScript(ulong seed)
        {
            var d = Create(seed);
            var winner = d.State.CoinTossWinner.Value;
            d.Ok(new ChooseTurnOrder(winner, goFirst: true));
            var other = winner == P1 ? P2 : P1;
            d.Place(winner, Fire180, winner, Forge);
            d.EndTurn(winner);
            d.Place(other, Water200, winner, Forge);
            d.Open(winner);
            d.Play(winner, Boost50);
            d.Ready(other);
            d.Ready(winner);
            return d.Log;
        }

        private static string Describe(IEnumerable<GameEvent> events) =>
            string.Join("\n", events.Select(Describe));

        private static string Describe(GameEvent gameEvent)
        {
            var properties = gameEvent.GetType().GetProperties()
                .OrderBy(property => property.Name)
                .Select(property => $"{property.Name}={Format(property.GetValue(gameEvent))}");
            return $"{gameEvent.GetType().Name}({string.Join(", ", properties)})";
        }

        private static string Format(object value) =>
            value is IEnumerable items && !(value is string)
                ? "[" + string.Join(",", items.Cast<object>()) + "]"
                : value?.ToString() ?? "null";
    }
}
