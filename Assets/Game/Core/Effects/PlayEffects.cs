using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>The host opens the battle's field at once if it is closed (and not locked).</summary>
    public sealed class OpenBattleFieldEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) => RejectReason.None;

        // TODO(rules): a locked field («Запертое поле») stays closed even under this effect. See docs/decisions.md.
        public void Apply(PlayContext context) => context.OpenBattleField();
    }

    /// <summary>Cancels the last common or epic ability the opponent played in this battle.</summary>
    public sealed class CancelLastOpponentAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) => RejectReason.None;

        public void Apply(PlayContext context) =>
            context.CancelLastOpponentAbility(ability => ability.Rarity != Rarity.Legendary);
    }

    /// <summary>You take the first turn in the upcoming battle. Only before the battle starts.</summary>
    public sealed class TakeFirstBattleTurnEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) =>
            context.IsBeforeBattle ? RejectReason.None : RejectReason.TimingNotAllowed;

        public void Apply(PlayContext context) => context.TakeFirstBattleTurn();
    }

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

    /// <summary>Your creature leaves the battle for your hand; the battle ends without a winner.</summary>
    public sealed class RetreatEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) =>
            context.Battle != null && !context.IsBeforeBattle ? RejectReason.None : RejectReason.TimingNotAllowed;

        public void Apply(PlayContext context) => context.Retreat();
    }

    /// <summary>Returns one of your creatures that is out of the game to your hand. Target: CardId.</summary>
    public sealed class ReturnCreatureFromOutOfGameEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var id = context.Targets.CardId;
            return id != null && context.State.Player(context.Caster).OutOfGame.Any(card => card.Id == id)
                ? RejectReason.None
                : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context) => context.ReturnCreatureFromOutOfGame(context.Targets.CardId.Value);
    }

    /// <summary>Returns one of your used abilities, other than this one, from the discard pile. Target: CardId.</summary>
    public sealed class ReturnAbilityFromDiscardEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var id = context.Targets.CardId;
            return id != null && id != context.Ability.Id &&
                   context.State.Player(context.Caster).Discard.Any(card => card.Id == id)
                ? RejectReason.None
                : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context) => context.ReturnAbilityFromDiscard(context.Targets.CardId.Value);
    }

    /// <summary>The opponent discards a random ability from hand.</summary>
    public sealed class DiscardRandomOpponentAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context) => RejectReason.None;

        public void Apply(PlayContext context) => context.DiscardRandomAbility(context.Opponent);
    }

    /// <summary>
    /// You discard one other ability from your hand; with none left, nothing happens.
    /// Target: CardId, required while you hold another ability.
    /// </summary>
    public sealed class DiscardOwnAbilityEffect : IPlayEffect
    {
        public RejectReason Validate(PlayContext context)
        {
            var others = context.AbilitiesInHand(context.Caster).Where(card => card.Id != context.Ability.Id).ToList();
            if (others.Count == 0)
                return RejectReason.None;
            return others.Any(card => card.Id == context.Targets.CardId) ? RejectReason.None : RejectReason.InvalidTarget;
        }

        public void Apply(PlayContext context)
        {
            var id = context.Targets.CardId;
            if (id != null && context.State.Player(context.Caster).FindInHand(id.Value) != null)
                context.DiscardFromHand(context.Caster, id.Value);
        }
    }

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

    /// <summary>Until the end of this battle the opponent cannot play common and epic abilities.</summary>
    public sealed class SilenceOpponentEffect : IAbilityRestriction
    {
        public bool Forbids(AbilityCard ability, PlayerId player, PlayerId caster) =>
            player != caster && ability.Rarity != Rarity.Legendary;
    }
}
