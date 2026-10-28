namespace Game.Core.Battle
{
    /// <summary>How a battle with equal final power got its winner (docs/rules.md §6).</summary>
    public enum TieBreak
    {
        /// <summary>The powers differed, or the battle is a draw.</summary>
        None,

        /// <summary>The creature placed on the field earlier won.</summary>
        EarlierPlacement,

        /// <summary>An ability («Перевес») gave the win.</summary>
        Ability,
    }
}
