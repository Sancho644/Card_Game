using System;
using Game.Core.Cards;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>
    /// "What if" counts for the current battle: a hypothetical change is made, counted and undone,
    /// so the state is the same afterwards. Used by forecasts.
    /// </summary>
    internal sealed class BattlePreview
    {
        private readonly MatchState _state;
        private readonly BattlePowerCounter _counter;

        public BattlePreview(MatchState state, BattlePowerCounter counter)
        {
            _state = state;
            _counter = counter;
        }

        public BattlePower WithAbility(PlayerId player, int abilityId)
        {
            var card = _state.Player(player).FindInHand(abilityId);
            if (card == null || !(card.Definition is AbilityCard))
                throw new ArgumentException($"Ability #{abilityId} is not in the hand of {player}.", nameof(abilityId));

            var battle = _state.Battle;
            var played = new PlayedAbility(player, card);
            battle.AddPlayedAbility(played);
            try
            {
                return _counter.Power(battle);
            }
            finally
            {
                battle.RemovePlayedAbility(played);
            }
        }

        public BattlePower WithOpenField()
        {
            var field = _state.Battle.Field;
            var wasOpen = field.IsOpen;
            field.IsOpen = true;
            try
            {
                return _counter.Power(_state.Battle);
            }
            finally
            {
                field.IsOpen = wasOpen;
            }
        }
    }
}
