using Game.Core.Events;
using Game.Core.State;
using NUnit.Framework;
using static Game.Core.Tests.TestCards;

namespace Game.Core.Tests
{
    /// <summary>
    /// The two examples from docs/rules.md §11. Anna owns the closed «Кузница»: Fire +60, Water −30.
    /// Anna's creature is Fire 180, Boris's creature is Water 200, the ability is a common +50.
    /// </summary>
    public class RulesExampleTests
    {
        private const PlayerId Anna = PlayerId.Player1;
        private const PlayerId Boris = PlayerId.Player2;

        [Test]
        public void Example1_BattleOnGuestTurn()
        {
            var d = MatchDriver.Begin(Anna);
            d.Place(Anna, Fire180, Anna, Forge);
            d.EndTurn(Anna);
            d.Place(Boris, Water200, Anna, Forge);

            Assert.AreEqual(Anna, d.Last<BattleStarted>().FirstActor, "Anna placed her creature earlier.");

            d.Play(Anna, Boost50);
            d.Ready(Boris);
            d.Ready(Anna);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(230, result.HostPower);
            Assert.AreEqual(200, result.GuestPower);
            Assert.AreEqual(Anna, result.Winner);

            var forge = d.Field(Anna, Forge);
            Assert.IsFalse(forge.IsOpen);
            Assert.IsFalse(forge.IsRemoved);
            Assert.AreEqual(Fire180, forge.OccupantOf(Anna).Definition.Id);
            Assert.IsTrue(d.IsOut(Boris, Water200));
            Assert.AreEqual(1, d.Wins(Anna));
        }

        [Test]
        public void Example2_BattleOnHostTurn()
        {
            var d = MatchDriver.Begin(Boris);
            d.Place(Boris, Water200, Anna, Forge);
            d.EndTurn(Boris);
            d.Place(Anna, Fire180, Anna, Forge);

            Assert.AreEqual(Boris, d.Last<BattleStarted>().FirstActor, "Boris's creature was on the card earlier.");

            d.Play(Boris, Boost50);
            d.Open(Anna);
            d.Ready(Anna);
            d.Ready(Boris);

            var result = d.Last<BattleResolved>();
            Assert.AreEqual(240, result.HostPower);
            Assert.AreEqual(220, result.GuestPower);
            Assert.AreEqual(Anna, result.Winner);

            Assert.IsTrue(d.Field(Anna, Forge).IsRemoved);
            Assert.IsTrue(d.InHand(Anna, Fire180));
            Assert.IsTrue(d.IsOut(Boris, Water200));
            Assert.AreEqual(1, d.Wins(Anna));
        }
    }
}
