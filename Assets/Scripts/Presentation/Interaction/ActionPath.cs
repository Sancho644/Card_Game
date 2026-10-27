using System.Collections.Generic;
using Game.Core.Commands;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// A legal command as a sequence of player choices: first the source card (if any), then each step.
    /// </summary>
    public sealed class ActionPath
    {
        /// <summary>Option value meaning "place without the signature ability".</summary>
        public const int NoOption = 0;

        public ActionPath(GameCommand command, int? source, IReadOnlyList<PathStep> steps)
        {
            Command = command;
            Source = source;
            Steps = steps;
        }

        public GameCommand Command { get; }

        /// <summary>The card the player starts with; null for a pure choice (e.g. «Трофей»).</summary>
        public int? Source { get; }

        public IReadOnlyList<PathStep> Steps { get; }
    }
}
