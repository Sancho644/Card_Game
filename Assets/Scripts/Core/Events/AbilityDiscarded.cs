using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>An ability went from a hand to the discard pile without being played.</summary>
    public sealed class AbilityDiscarded : GameEvent
    {
        public AbilityDiscarded(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }
}
