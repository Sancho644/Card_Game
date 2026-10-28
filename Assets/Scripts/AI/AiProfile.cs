namespace Game.AI
{
    /// <summary>A difficulty level: how carefully the AI looks at the position, filled from AiConfig.</summary>
    public sealed class AiProfile
    {
        public string Id { get; set; } = "normal";

        /// <summary>Guesses about the opponent's hidden cards per decision; more is steadier and slower.</summary>
        public int Samples { get; set; } = 3;

        /// <summary>Random spread added to every action's score; makes the AI weaker and less predictable.</summary>
        public double Noise { get; set; }

        /// <summary>Whether the AI expects the opponent to answer with abilities from hand.</summary>
        public bool ExpectsOpponentResponse { get; set; } = true;

        /// <summary>
        /// Whether the AI values the score as the chance to win the match from wins and creatures left
        /// (<see cref="MatchEquityValue"/>) instead of flat points per win.
        /// </summary>
        public bool UsesMatchEquity { get; set; } = true;

        /// <summary>Whether the AI looks at the opponent's best answer to each action: in a battle or its next main action.</summary>
        public bool ConsidersReplies { get; set; } = true;

        /// <summary>Whether the AI plays abilities on the opponent's turn.</summary>
        public bool UsesReactions { get; set; } = true;

        /// <summary>A reaction is played only if it gains more than this.</summary>
        public double ReactionThreshold { get; set; } = 5;

        public bool PrefersFirstTurn { get; set; } = true;

        public float ThinkDelayMin { get; set; } = 0.6f;
        public float ThinkDelayMax { get; set; } = 1.2f;

        public AiWeights Weights { get; set; } = new AiWeights();
    }
}
