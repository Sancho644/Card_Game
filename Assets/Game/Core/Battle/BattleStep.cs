namespace Game.Core.Battle
{
    /// <summary>
    /// When an ability effect applies during the power count (docs/rules.md §6, «Подсчёт силы»).
    /// Effects of the same step apply in play order.
    /// </summary>
    public enum BattleStep
    {
        /// <summary>Before everything: cancelling the field condition, whenever it was played.</summary>
        ConditionControl,

        /// <summary>Step 1: changing the field card's values, e.g. swapping bonuses and penalties.</summary>
        FieldValues,

        /// <summary>Step 2: the other abilities, in play order.</summary>
        Power,
    }
}
