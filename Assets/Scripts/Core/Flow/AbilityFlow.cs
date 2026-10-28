using System.Linq;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.Effects;
using Game.Core.Events;
using Game.Core.State;

namespace Game.Core.Flow
{
    /// <summary>Checks and plays abilities: timing, limits, conditions, targets, then play effects.</summary>
    internal sealed class AbilityFlow
    {
        private readonly MatchContext _match;

        public AbilityFlow(MatchContext match)
        {
            _match = match;
        }

        private MatchState State => _match.State;

        /// <summary>The moments at which the player may play an ability right now.</summary>
        public AbilityTiming AvailableMoments(PlayerId player)
        {
            if (State.Phase == MatchPhase.Battle)
                return player == State.Battle.CurrentActor ? AbilityTiming.InBattle : AbilityTiming.InBattleOnOpponentTurn;
            if (State.Phase != MatchPhase.Turn || player != State.ActivePlayer)
                return AbilityTiming.None;

            var moments = State.MainActionDone ? AbilityTiming.AfterMainAction : AbilityTiming.BeforeMainAction;
            if (State.BattleHappenedThisTurn)
                moments |= AbilityTiming.AfterBattle;
            return moments;
        }

        /// <param name="battleCreature">The player's creature in the battle; null outside a battle.</param>
        /// <param name="isBeforeBattle">Played together with a placement, before the battle is announced.</param>
        public RejectReason Validate(
            PlayerId player,
            int abilityId,
            AbilityTargets targets,
            AbilityTiming moments,
            BattleState battle,
            CardDefinition battleCreature,
            bool isBeforeBattle)
        {
            var playerState = State.Player(player);
            var card = playerState.FindInHand(abilityId);
            if (card == null)
                return RejectReason.CardNotInHand;
            if (!(card.Definition is AbilityCard ability))
                return RejectReason.NotAnAbility;
            if (!ability.CanBePlayedAt(moments))
                return moments == AbilityTiming.InBattleOnOpponentTurn ? RejectReason.NotYourTurn : RejectReason.TimingNotAllowed;
            if (ability.Rarity == Rarity.Legendary &&
                playerState.LegendariesPlayedThisTurn >= _match.Rules.LegendaryAbilitiesPerTurn)
                return RejectReason.LegendaryLimitReached;
            if (ability.IsSignature && battleCreature?.Id != ability.BoundCreatureId)
                return RejectReason.SignatureCreatureNotInBattle;

            if (battle != null)
            {
                if (battle.IsConditionActive &&
                    battle.Field.Definition.Conditions.Any(condition => !condition.AllowsAbility(ability, player, battle)))
                    return RejectReason.BlockedByFieldCondition;
                if (battle.IsForbiddenByAbilities(ability, player))
                    return RejectReason.BlockedByAbility;
            }

            var context = new PlayContext(_match, player, card, targets, battle, isBeforeBattle);
            foreach (var effect in ability.Effects.OfType<IPlayEffect>())
            {
                var reason = effect.Validate(context);
                if (reason != RejectReason.None)
                    return reason;
            }

            return RejectReason.None;
        }

        /// <summary>
        /// Checks a signature ability played together with placing (or moving) its creature on the field.
        /// The placement must start a battle; the creature is not on the field yet.
        /// </summary>
        public RejectReason ValidateWithPlacement(PlayerId player, int abilityId, FieldSlot field, CardInstance creature)
        {
            if (field.OccupantOf(player.Opponent()) == null)
                return RejectReason.SignatureNeedsBattle;

            var upcomingBattle = new BattleState(field, player);
            return Validate(player, abilityId, AbilityTargets.None, AbilityTiming.BeforeBattle,
                upcomingBattle, creature.Definition, isBeforeBattle: true);
        }

        /// <summary>Starts the battle on a field just made contested, playing the signature ability first if given.</summary>
        public void StartBattle(PlayerId player, FieldSlot field, int? signatureAbilityId)
        {
            if (signatureAbilityId == null)
            {
                _match.Battles.StartIfContested(field);
                return;
            }

            var battle = _match.Battles.Open(field);
            Play(player, signatureAbilityId.Value, AbilityTargets.None, battle, isBeforeBattle: true);
            _match.Battles.Announce(battle);
        }

        /// <summary>Spends the card and applies its play effects. Call only after <see cref="Validate"/>.</summary>
        public void Play(PlayerId player, int abilityId, AbilityTargets targets, BattleState battle, bool isBeforeBattle)
        {
            var playerState = State.Player(player);
            var card = playerState.FindInHand(abilityId);
            var ability = (AbilityCard)card.Definition;

            // The card goes to the discard pile at once; its effect lasts until the battle ends (docs/rules.md §6).
            playerState.RemoveFromHand(card);
            playerState.AddToDiscard(card);
            if (ability.Rarity == Rarity.Legendary)
                playerState.LegendariesPlayedThisTurn++;
            battle?.AddPlayedAbility(new PlayedAbility(player, card));
            _match.Events.Emit(new AbilityPlayed(player, CardRef.From(card)));

            var context = new PlayContext(_match, player, card, targets, battle, isBeforeBattle);
            foreach (var effect in ability.Effects.OfType<IPlayEffect>())
                effect.Apply(context);
        }
    }
}
