using Game.Core.Commands;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>
    /// Moves a creature of the given side to another field where that side has no creature.
    /// Targets: <see cref="AbilityTargets.CreatureId"/> and <see cref="AbilityTargets.FieldId"/>.
    /// </summary>
    public sealed class MoveCreatureEffect : IPlayEffect
    {
        private readonly bool _ownCreature;

        /// <param name="ownCreature">True: move your creature («Манёвр»); false: the opponent's («Смещение»).</param>
        public MoveCreatureEffect(bool ownCreature)
        {
            _ownCreature = ownCreature;
        }

        public AbilityTargetKinds Targets => AbilityTargetKinds.Creature | AbilityTargetKinds.Field;

        public RejectReason Validate(PlayContext context)
        {
            var owner = Owner(context);
            var targets = context.Targets;
            if (targets.CreatureId == null || targets.FieldId == null)
                return RejectReason.InvalidTarget;
            if (context.State.FindFieldWith(owner, targets.CreatureId.Value) == null)
                return RejectReason.InvalidTarget;

            var field = context.State.FindField(targets.FieldId.Value);
            if (field == null || field.IsRemoved || field.OccupantOf(owner) != null)
                return RejectReason.InvalidTarget;
            return RejectReason.None;
        }

        public void Apply(PlayContext context) =>
            context.MoveCreature(Owner(context), context.Targets.CreatureId.Value, context.Targets.FieldId.Value);

        private PlayerId Owner(PlayContext context) => _ownCreature ? context.Caster : context.Opponent;
    }
}
