using System;

namespace Game.Core.Cards
{
    public enum Element
    {
        Light,
        Dark,
        Earth,
        Water,
        Fire,
        Wind,
    }

    public enum Rarity
    {
        Common,
        Epic,
        Legendary,
    }

    public enum CardType
    {
        Field,
        Creature,
        Ability,
    }

    /// <summary>When an ability may be played. The card text decides; flags can be combined.</summary>
    [Flags]
    public enum AbilityTiming
    {
        None = 0,
        BeforeMainAction = 1 << 0,
        BeforeBattle = 1 << 1,
        InBattle = 1 << 2,
        InBattleOnOpponentTurn = 1 << 3,
        AfterBattle = 1 << 4,
    }
}
