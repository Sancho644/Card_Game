using System.Collections.Generic;
using System.Linq;
using Game.Core.Commands;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Turns the engine's legal commands into choice paths. Button commands (end turn, ready, open field,
    /// turn order) are not paths; the action bar shows them directly.
    /// </summary>
    public sealed class ActionPathBuilder
    {
        public IReadOnlyList<ActionPath> Build(IEnumerable<GameCommand> commands)
        {
            var paths = new List<ActionPath>();
            var placements = new List<(GameCommand command, int creature, int field, int? signature)>();

            foreach (var command in commands)
            {
                switch (command)
                {
                    case PlaceCreature place:
                        placements.Add((place, place.CreatureId, place.FieldId, place.SignatureAbilityId));
                        break;
                    case Challenge challenge:
                        placements.Add((challenge, challenge.CreatureId, challenge.TargetFieldId, challenge.SignatureAbilityId));
                        break;
                    case PlayAbility play:
                        paths.Add(new ActionPath(play, play.AbilityId, AbilitySteps(play.Targets)));
                        break;
                    case ChooseCard choose:
                        paths.Add(new ActionPath(choose, null, new[] { new PathStep(PathStepKind.Option, choose.CardId) }));
                        break;
                }
            }

            // A signature ability can go with the placement: then "with which ability, or none" is one more step.
            foreach (var group in placements.GroupBy(placement => (placement.creature, placement.field)))
            {
                var withSignature = group.Any(placement => placement.signature != null);
                foreach (var placement in group)
                {
                    var steps = new List<PathStep> { new PathStep(PathStepKind.Card, placement.field) };
                    if (withSignature)
                        steps.Add(new PathStep(PathStepKind.Option, placement.signature ?? ActionPath.NoOption));
                    paths.Add(new ActionPath(placement.command, placement.creature, steps));
                }
            }

            return paths;
        }

        private static IReadOnlyList<PathStep> AbilitySteps(AbilityTargets targets)
        {
            var steps = new List<PathStep>();
            if (targets.CreatureId != null)
                steps.Add(new PathStep(PathStepKind.Card, targets.CreatureId.Value));
            if (targets.FieldId != null)
                steps.Add(new PathStep(PathStepKind.Card, targets.FieldId.Value));
            if (targets.CardId != null)
                steps.Add(new PathStep(PathStepKind.Option, targets.CardId.Value));
            return steps;
        }
    }
}
