using Game.Core.Commands;

namespace Game.Core.Effects
{
    /// <summary>Locks one of the opponent's closed fields: the host cannot open it in the next battle on it. Target: FieldId.</summary>
    public sealed class LockFieldEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var id = context.Targets.FieldId;
            var field = id == null ? null : context.State.FindField(id.Value);
            return field != null && ScoutEffect.IsClosedFieldOf(field, context.Opponent)
                ? RejectReason.None
                : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context) => context.LockField(context.Targets.FieldId.Value);
    }
}
