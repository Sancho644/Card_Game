namespace Game.Simulation
{
    /// <summary>How decks are built when the simulation has no fixed decks.</summary>
    public enum SimulationDeckMode
    {
        /// <summary>Every slot gets a random fitting card.</summary>
        Random,

        /// <summary>Creatures are picked to suit the bonuses of the deck's own fields, with a little randomness.</summary>
        Synergy,
    }
}
