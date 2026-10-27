using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>One change of a creature's power in step 2 of the power count, and the ability that made it.</summary>
    public sealed class PowerContribution
    {
        public PowerContribution(CardRef source, int amount)
        {
            Source = source;
            Amount = amount;
        }

        /// <summary>The ability card, or null if the change came from elsewhere.</summary>
        public CardRef Source { get; }

        public int Amount { get; }
    }
}
