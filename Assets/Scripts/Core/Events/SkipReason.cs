namespace Game.Core.Events
{
    /// <summary>Why a player had no main action and the turn counted as a skip.</summary>
    public enum SkipReason
    {
        /// <summary>The player has a main action; nothing is skipped.</summary>
        None,

        /// <summary>Creatures in hand, but every field in play already holds one of the player's creatures.</summary>
        NoFreeField,

        /// <summary>No creatures in hand and none on the fields.</summary>
        NoCreatures,

        /// <summary>No creatures in hand, and no field holds an opponent's creature alone to challenge.</summary>
        NoChallengeTarget,
    }
}
