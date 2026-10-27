using System.Linq;
using Game.Core.Cards;
using Game.Core.Events;
using Game.Core.Flow;
using Game.Core.State;

namespace Game.Core.Commands.Handlers
{
    /// <summary>
    /// Playing an ability during the player's battle turn. The turn then passes to the opponent,
    /// and the opponent's readiness is reset.
    /// Windows outside battle and plays on the opponent's turn come with the legendary effects.
    /// </summary>
    internal sealed class PlayAbilityHandler : CommandHandler<PlayAbility>
    {
        protected override RejectReason Validate(PlayAbility command, MatchContext context)
        {
            var state = context.State;
            var battle = state.Battle;
            if (state.Phase != MatchPhase.Battle)
                return RejectReason.TimingNotAllowed;
            if (battle.CurrentActor != command.Player)
                return RejectReason.NotYourTurn;

            var player = state.Player(command.Player);
            var card = player.FindInHand(command.AbilityId);
            if (card == null)
                return RejectReason.CardNotInHand;
            if (!(card.Definition is AbilityCard ability))
                return RejectReason.NotAnAbility;
            if (!ability.CanBePlayedAt(AbilityTiming.InBattle))
                return RejectReason.TimingNotAllowed;
            if (ability.Rarity == Rarity.Legendary &&
                player.LegendariesPlayedThisTurn >= context.Rules.LegendaryAbilitiesPerTurn)
                return RejectReason.LegendaryLimitReached;
            if (ability.IsSignature &&
                battle.Field.OccupantOf(command.Player).Definition.Id != ability.BoundCreatureId)
                return RejectReason.SignatureCreatureNotInBattle;
            if (battle.Field.IsOpen &&
                battle.Field.Definition.Conditions.Any(condition => !condition.AllowsAbility(ability, command.Player, battle)))
                return RejectReason.BlockedByFieldCondition;

            return RejectReason.None;
        }

        protected override void Apply(PlayAbility command, MatchContext context)
        {
            var player = context.State.Player(command.Player);
            var card = player.FindInHand(command.AbilityId);

            // TODO(rules): the card goes to the discard pile at once; its effect lasts until the battle ends.
            // See docs/decisions.md.
            player.RemoveFromHand(card);
            player.AddToDiscard(card);
            if (card.Definition.Rarity == Rarity.Legendary)
                player.LegendariesPlayedThisTurn++;

            context.State.Battle.AddPlayedAbility(new PlayedAbility(command.Player, card));
            context.Events.Emit(new AbilityPlayed(command.Player, CardRef.From(card)));
            context.Battles.ResetReadiness(command.Player.Opponent());
            context.Battles.PassTurn();
        }
    }
}
