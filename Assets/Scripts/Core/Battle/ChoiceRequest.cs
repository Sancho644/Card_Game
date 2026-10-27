using System;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>A choice for a player after the battle; options are the player's cards matching the filter.</summary>
    public sealed class ChoiceRequest
    {
        public ChoiceRequest(PlayerId player, ChoicePurpose purpose, Func<CardInstance, bool> filter)
        {
            Player = player;
            Purpose = purpose;
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public Func<CardInstance, bool> Filter { get; }
    }
}
