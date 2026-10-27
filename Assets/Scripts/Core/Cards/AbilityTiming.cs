using System;

namespace Game.Core.Cards
{
    /// <summary>When an ability may be played. The card text decides; flags can be combined.</summary>
    [Flags]
    public enum AbilityTiming
    {
        None = 0,

        /// <summary>Own turn, before the main action.</summary>
        BeforeMainAction = 1 << 0,

        /// <summary>Together with placing its creature, before the battle starts (signature abilities).</summary>
        BeforeBattle = 1 << 1,

        /// <summary>Own turn in a battle.</summary>
        InBattle = 1 << 2,

        /// <summary>The opponent's turn in a battle.</summary>
        InBattleOnOpponentTurn = 1 << 3,

        /// <summary>Own turn, after the main action.</summary>
        AfterMainAction = 1 << 4,

        /// <summary>Own turn, after a battle that took place in this turn.</summary>
        AfterBattle = 1 << 5,
    }
}
