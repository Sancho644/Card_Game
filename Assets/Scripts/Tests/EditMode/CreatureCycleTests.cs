using System.Linq;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.Rules;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.MatchDriver;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    public sealed class CreatureCycleTests
    {
        [TestCase(3, false)]
        [TestCase(10, true)]
        public void ThirdBattle_ReturnsWholeGroupUnlessMatchEnded(int wins, bool returns)
        {
            var d = Begin(P1, rules: new GameRules(wins, 1, DeckRules.Default));
            foreach (var creature in new[] { Fire180, Water200, Earth250 })
            {
                d.Place(P1, creature, P1, Forge); d.EndTurn(P1);
                d.Place(P2, creature, P1, Forge); d.Ready(P1); d.Ready(P2);
                if (creature != Earth250)
                {
                    Assert.IsTrue(d.IsOut(P1, creature));
                    Assert.IsTrue(d.IsOut(P2, creature));
                    Assert.IsFalse(d.Log.OfType<CreatureReturnedToHand>().Any());
                    d.EndTurn(P2);
                }
            }
            foreach (var owner in PlayerIds.All)
            {
                Assert.AreEqual(returns ? 3 : 0, d.State.Player(owner).Hand.Count(c => c.Type == CardType.Creature));
                Assert.AreEqual(returns ? 0 : 3, d.State.Player(owner).OutOfGame.Count);
            }
            Assert.AreEqual(returns ? 6 : 0, d.Log.OfType<CreatureReturnedToHand>().Count());
            if (returns)
            {
                d.EndTurn(P2); d.Place(P1, Fire180, P1, Forge);
                Assert.IsNotNull(d.Field(P1, Forge).OccupantOf(P1));
            }
        }

        [Test]
        public void EmptyHand_WithCreatureOnTable_DoesNotReturnGroup()
        {
            var d = Begin(P1, rules: new GameRules(10, 1, DeckRules.Default));
            foreach (var creature in new[] { Fire180, Water200 })
            {
                d.Place(P1, creature, P1, Forge); d.EndTurn(P1);
                d.Place(P2, creature, P1, Forge); d.Ready(P1); d.Ready(P2); d.EndTurn(P2);
            }
            d.Place(P1, Earth250, P1, Forge);
            Assert.IsFalse(d.State.Player(P1).HasCreatureInHand);
            Assert.AreEqual(2, d.State.Player(P1).OutOfGame.Count);
            Assert.IsEmpty(d.Log.OfType<CreatureReturnedToHand>());
        }
    }
}
