using System.Collections.Generic;

namespace Game.Core.State
{
    public sealed class PendingChoice
    {
        internal PendingChoice(PlayerId player, ChoicePurpose purpose, IReadOnlyList<CardInstance> options)
        {
            Player = player;
            Purpose = purpose;
            Options = options;
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public IReadOnlyList<CardInstance> Options { get; }
    }
}
