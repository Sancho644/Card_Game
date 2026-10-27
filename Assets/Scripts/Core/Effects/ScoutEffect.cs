using System.Linq;
using Game.Core.Commands;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// You see the opponent's abilities in hand and one of their closed fields.
    /// Target: FieldId, required while the opponent has a closed field.
    /// </summary>
    public sealed class ScoutEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var closedFields = context.State.Fields.Where(field => IsClosedFieldOf(field, context.Opponent)).ToList();
            if (closedFields.Count == 0)
                return RejectReason.None;
            return closedFields.Any(field => field.Id == context.Targets.FieldId) ? RejectReason.None : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context)
        {
            var field = context.Targets.FieldId == null ? null : context.State.FindField(context.Targets.FieldId.Value);
            context.RevealToCaster(field);
        }

        internal static bool IsClosedFieldOf(FieldSlot field, PlayerId owner) =>
            field.Owner == owner && !field.IsOpen && !field.IsRemoved;
    }
}
