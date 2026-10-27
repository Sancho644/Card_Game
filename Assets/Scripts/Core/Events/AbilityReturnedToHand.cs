using Game.Core.State;

namespace Game.Core.Events
{
    /// <summary>A used ability came back from the discard pile to the hand.</summary>
    public sealed class AbilityReturnedToHand : GameEvent
    {
        public AbilityReturnedToHand(PlayerId owner, CardRef ability)
        {
            Owner = owner;
            Ability = ability;
        }

        public PlayerId Owner { get; }
        public CardRef Ability { get; }
    }
}
