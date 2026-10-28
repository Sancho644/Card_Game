using System.Collections.Generic;
using Game.AI;
using Game.Core.Decks;

namespace Game.Simulation
{
    /// <summary>One batch of AI-versus-AI matches.</summary>
    public sealed class SimulationOptions
    {
        /// <summary>Shown in the report, e.g. "Обычный против Обычного".</summary>
        public string Name { get; set; } = "Simulation";

        public int Games { get; set; } = 1000;

        /// <summary>Master seed; every match gets its own seed derived from it.</summary>
        public ulong Seed { get; set; } = 1;

        /// <summary>Decks to pick from for each player; empty means a random valid deck every match.</summary>
        public IReadOnlyList<DeckList> Decks { get; set; } = new DeckList[0];

        public AiProfile Player1Profile { get; set; } = new AiProfile();
        public AiProfile Player2Profile { get; set; } = new AiProfile();

        /// <summary>Swap the profiles between the seats every other match.</summary>
        public bool AlternateSeats { get; set; }

        /// <summary>A match still running after this many turns is recorded as hung.</summary>
        public int TurnLimit { get; set; } = 200;
    }
}
