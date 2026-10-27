using System.Linq;
using Game.Core.Battle;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Presentation.Cards;
using Game.Presentation.Match;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    /// <summary>Power changes are reported per source: the field's value and each ability.</summary>
    public class PowerDiffTests
    {
        private const int Field = 30;

        private PowerDiff _diff;

        [SetUp]
        public void SetUp()
        {
            var faces = new CardFaceBuilder(
                ScriptableObject.CreateInstance<CardDatabase>(),
                ScriptableObject.CreateInstance<VisualSettings>(),
                ScriptableObject.CreateInstance<UiTexts>());
            _diff = new PowerDiff(new CardNameLookup(faces));
        }

        [Test]
        public void FieldOpened_ReportsFieldValue()
        {
            var changes = _diff.Changes(Power(0), Power(60), Field);

            Assert.AreEqual(60, changes.Single().Amount);
        }

        [Test]
        public void AbilityPlayed_ReportsItsContribution_FieldUnchanged()
        {
            var changes = _diff.Changes(Power(60), Power(60, (5, 50)), Field);

            Assert.AreEqual(50, changes.Single().Amount);
        }

        [Test]
        public void AbilityCancelled_ReportsContributionTakenBack()
        {
            var changes = _diff.Changes(Power(0, (5, 50), (6, -20)), Power(0, (6, -20)), Field);

            Assert.AreEqual(-50, changes.Single().Amount);
        }

        [Test]
        public void SeveralContributionsOfOneAbility_AreSummed()
        {
            var changes = _diff.Changes(Power(0), Power(0, (5, 30), (5, 20)), Field);

            Assert.AreEqual(50, changes.Single().Amount);
        }

        [Test]
        public void NothingChanged_ReportsNothing()
        {
            CollectionAssert.IsEmpty(_diff.Changes(Power(60, (5, 50)), Power(60, (5, 50)), Field));
        }

        private static PowerBreakdown Power(int fieldValue, params (int ability, int amount)[] abilities) =>
            new PowerBreakdown(
                200,
                fieldValue,
                abilities.Select(a => new PowerContribution(new CardRef(a.ability, "ability." + a.ability), a.amount)).ToList(),
                200 + fieldValue + abilities.Sum(a => a.amount));
    }
}
