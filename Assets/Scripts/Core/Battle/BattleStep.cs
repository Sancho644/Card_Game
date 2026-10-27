namespace Game.Core.Battle
{
    /// <summary>
    /// When a battle effect applies during the power count (docs/rules.md §6, «Подсчёт силы»).
    /// Effects of the same step apply in play order. Condition cancellation is not a step:
    /// a cancelled condition acts at no step at all.
    /// </summary>
    public enum BattleStep
    {
        /// <summary>Step 1: changing the field card's values, e.g. swapping bonuses and penalties.</summary>
        FieldValues,

        /// <summary>Step 2: the other abilities, in play order.</summary>
        Power,
    }
}
