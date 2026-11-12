using System;
using System.Linq;
using Game.Core;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.State;

namespace Game.AI
{
    /// <summary>Values a deployed field through possible battles, using only a rebuilt forecast.</summary>
    internal sealed class FieldDeploymentScorer
    {
        private readonly PositionEvaluator _evaluator;

        public FieldDeploymentScorer(PositionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public double Score(Func<MatchEngine> build, ChooseCard choice, PlayerId me)
        {
            var initial = build().State.PendingChoice.Purpose == ChoicePurpose.InitialField;
            var position = Prepare(build, choice, me);
            var own = Creatures(position, me);
            var opponents = Creatures(position, me.Opponent());
            if (own.Length == 0 || opponents.Length == 0)
                return _evaluator.Evaluate(position, me);

            var starters = initial ? PlayerIds.All.ToArray() : new[] { position.State.ActivePlayer };
            // The owner can choose a creature suited to this field, against different possible opponents.
            return starters.Average(first => own.Max(creature => opponents.Average(opponent =>
                Pair(build, choice, me, first, creature, opponent))));
        }

        private double Pair(Func<MatchEngine> build, ChooseCard choice, PlayerId me,
            PlayerId first, int ownCreature, int opponentCreature)
        {
            var engine = Prepare(build, choice, first);
            var firstCreature = first == me ? ownCreature : opponentCreature;
            var secondCreature = first == me ? opponentCreature : ownCreature;
            Apply(engine, new PlaceCreature(first, firstCreature, choice.CardId));
            Apply(engine, new EndTurn(first));
            Apply(engine, new PlaceCreature(first.Opponent(), secondCreature, choice.CardId));

            // Evaluate the actual closed and open consequences separately, including field conditions.
            // The generic battle evaluator normally anticipates opening a closed field itself.
            var closed = _evaluator.Evaluate(engine, me, considerOpening: false);
            if (engine.State.Battle.CurrentActor != me)
                Apply(engine, new Ready(engine.State.Battle.CurrentActor));
            var opened = engine.Execute(new OpenField(me));
            return opened.IsAccepted
                ? Math.Max(closed, _evaluator.Evaluate(engine, me, considerOpening: false))
                : closed;
        }

        private static MatchEngine Prepare(Func<MatchEngine> build, ChooseCard choice, PlayerId first)
        {
            var engine = build();
            Apply(engine, choice);
            // Finish only the deployment round. The other field does not participate in this probe.
            while (engine.State.Phase == MatchPhase.AwaitingChoice)
            {
                var pending = engine.State.PendingChoice;
                if (pending.Purpose != ChoicePurpose.InitialField && pending.Purpose != ChoicePurpose.ReplacementField)
                    throw new InvalidOperationException("Unexpected choice during field deployment forecast.");
                var card = pending.Options.OrderBy(c => c.Definition.Id, StringComparer.Ordinal).First();
                Apply(engine, new ChooseCard(pending.Player, card.Id));
            }
            if (engine.State.Phase == MatchPhase.ChoosingTurnOrder)
            {
                var winner = engine.State.CoinTossWinner.Value;
                Apply(engine, new ChooseTurnOrder(winner, winner == first));
            }
            return engine;
        }

        private static int[] Creatures(MatchEngine engine, PlayerId player) =>
            engine.State.Player(player).Hand.Where(c => c.Type == CardType.Creature)
                .OrderBy(c => c.Definition.Id, StringComparer.Ordinal).Select(c => c.Id).ToArray();

        private static void Apply(MatchEngine engine, GameCommand command)
        {
            var result = engine.Execute(command);
            if (!result.IsAccepted)
                throw new InvalidOperationException("Field deployment forecast rejected " + command.GetType().Name + ": " + result.Reason);
        }
    }
}
