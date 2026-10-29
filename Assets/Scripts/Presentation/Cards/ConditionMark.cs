namespace Game.Presentation.Cards
{
    /// <summary>What the condition badge of a field card shows.</summary>
    public enum ConditionMark
    {
        /// <summary>No battle on the open field, or the field has no condition.</summary>
        None,

        /// <summary>The field is open in the current battle and its condition acts.</summary>
        Active,

        /// <summary>The field is open in the current battle, but its condition is cancelled («Разрыв условия»).</summary>
        Cancelled,
    }
}
