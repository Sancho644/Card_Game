using System.Collections.Generic;
using Game.Core.Commands;
using Game.Core.State;
using Game.Presentation.Interaction;
using NUnit.Framework;

namespace Game.Presentation.Tests
{
    /// <summary>The selector walks through the engine's legal commands step by step and never invents one.</summary>
    public class ActionSelectorTests
    {
        private const PlayerId Player = PlayerId.Player1;

        private ActionSelector _selector;
        private GameCommand _chosen;
        private IReadOnlyList<int> _options;

        [SetUp]
        public void SetUp()
        {
            _selector = new ActionSelector();
            _chosen = null;
            _options = null;
            _selector.CommandChosen += command => _chosen = command;
            _selector.OptionsRequested += options => _options = options;
        }

        [Test]
        public void Placement_CreatureThenField()
        {
            var target = new PlaceCreature(Player, 1, 20);
            _selector.SetActions(new GameCommand[]
            {
                new PlaceCreature(Player, 1, 10), target, new PlaceCreature(Player, 2, 10), new EndTurn(Player),
            });

            CollectionAssert.AreEquivalent(new[] { 1, 2 }, _selector.Sources);
            _selector.TapCard(1);
            CollectionAssert.AreEquivalent(new[] { 10, 20 }, _selector.Targets);
            _selector.TapCard(20);

            Assert.AreSame(target, _chosen);
        }

        [Test]
        public void Placement_WithSignatureVariant_AsksWhichAbility()
        {
            var withSignature = new PlaceCreature(Player, 1, 10, signatureAbilityId: 7);
            _selector.SetActions(new GameCommand[] { new PlaceCreature(Player, 1, 10), withSignature });

            _selector.TapCard(1);
            _selector.TapCard(10);
            CollectionAssert.AreEquivalent(new[] { ActionPath.NoOption, 7 }, _options);
            _selector.ChooseOption(7);

            Assert.AreSame(withSignature, _chosen);
        }

        [Test]
        public void AbilityWithoutTargets_IsChosenOnTap()
        {
            var play = new PlayAbility(Player, 5);
            _selector.SetActions(new GameCommand[] { play, new Ready(Player) });

            _selector.TapCard(5);

            Assert.AreSame(play, _chosen);
        }

        [Test]
        public void AbilityWithCreatureAndField_TakesBothSteps()
        {
            var target = new PlayAbility(Player, 5, new AbilityTargets(creatureId: 1, fieldId: 20));
            _selector.SetActions(new GameCommand[]
            {
                new PlayAbility(Player, 5, new AbilityTargets(creatureId: 1, fieldId: 10)),
                target,
                new PlayAbility(Player, 5, new AbilityTargets(creatureId: 2, fieldId: 30)),
            });

            _selector.TapCard(5);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, _selector.Targets);
            _selector.TapCard(1);
            CollectionAssert.AreEquivalent(new[] { 10, 20 }, _selector.Targets);
            Assert.IsNull(_chosen);
            _selector.TapCard(20);

            Assert.AreSame(target, _chosen);
        }

        [Test]
        public void PendingChoice_OffersOptionsAtOnce()
        {
            var second = new ChooseCard(Player, 9);
            _selector.SetActions(new GameCommand[] { new ChooseCard(Player, 8), second });

            CollectionAssert.AreEquivalent(new[] { 8, 9 }, _options);
            _selector.ChooseOption(9);

            Assert.AreSame(second, _chosen);
        }

        [Test]
        public void TapOnCardWithoutAction_DoesNothing_AndCancelResets()
        {
            _selector.SetActions(new GameCommand[] { new PlaceCreature(Player, 1, 10) });

            _selector.TapCard(99);
            Assert.IsFalse(_selector.IsSelecting);

            _selector.TapCard(1);
            Assert.IsTrue(_selector.IsSelecting);
            _selector.Cancel();
            Assert.IsFalse(_selector.IsSelecting);
            Assert.IsNull(_chosen);
        }

        [Test]
        public void ButtonCommands_AreNotTableSources()
        {
            _selector.SetActions(new GameCommand[] { new EndTurn(Player), new Ready(Player), new OpenField(Player) });

            CollectionAssert.IsEmpty(_selector.Sources);
            Assert.IsNull(_options);
        }
    }
}
