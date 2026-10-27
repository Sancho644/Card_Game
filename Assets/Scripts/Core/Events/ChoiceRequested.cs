using System.Collections.Generic;
using Game.Core.State;

namespace Game.Core.Events
{
    public sealed class ChoiceRequested : GameEvent
    {
        public ChoiceRequested(PlayerId player, ChoicePurpose purpose, IReadOnlyList<CardRef> options)
        {
            Player = player;
            Purpose = purpose;
            Options = options;
        }

        public PlayerId Player { get; }
        public ChoicePurpose Purpose { get; }
        public IReadOnlyList<CardRef> Options { get; }
    }
}
