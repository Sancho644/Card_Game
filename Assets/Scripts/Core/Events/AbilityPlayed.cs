using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class AbilityPlayed : GameEvent
    {
        public AbilityPlayed(PlayerId player, CardRef ability)
        {
            Player = player;
            Ability = ability;
        }

        public PlayerId Player { get; }
        public CardRef Ability { get; }
    }
}
