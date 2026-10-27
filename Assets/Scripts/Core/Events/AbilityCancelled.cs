using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class AbilityCancelled : GameEvent
    {
        public AbilityCancelled(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }
}
