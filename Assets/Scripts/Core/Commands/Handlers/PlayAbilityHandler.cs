using Game.Core.Cards;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    /// <summary>
    /// Playing an ability in a turn window or in a battle. On the player's own battle turn the turn then
    /// passes to the opponent and the opponent's readiness is reset; a play on the opponent's turn
    /// (where the card allows it) changes neither.
    /// </summary>
    internal sealed class PlayAbilityHandler : CommandHandler<PlayAbility>
    {
        protected override RejectReason Validate(PlayAbility command, MatchContext context)
        {
            var state = context.State;
            if (state.Phase != MatchPhase.Turn && state.Phase != MatchPhase.Battle)
                return RejectReason.WrongPhase;

            var moments = context.Abilities.AvailableMoments(command.Player);
            if (moments == AbilityTiming.None)
                return RejectReason.NotYourTurn;

            var battle = state.Battle;
            var battleCreature = battle?.Field.OccupantOf(command.Player).Definition;
            return context.Abilities.Validate(
                command.Player, command.AbilityId, command.Targets, moments, battle, battleCreature, isBeforeBattle: false);
        }

        protected override void Apply(PlayAbility command, MatchContext context)
        {
            var battle = context.State.Battle;
            var onOwnBattleTurn = battle != null && battle.CurrentActor == command.Player;

            context.Abilities.Play(command.Player, command.AbilityId, command.Targets, battle, isBeforeBattle: false);

            if (onOwnBattleTurn && context.State.Battle == battle)
            {
                context.Battles.ResetReadiness(command.Player.Opponent());
                context.Battles.PassTurn();
            }
        }
    }
}
