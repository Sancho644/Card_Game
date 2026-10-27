using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Commands;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>One effect printed on an ability card. A concrete effect implements one of the interfaces below.</summary>
    public interface IAbilityEffect
    {
    }

    /// <summary>Applied during the power count at its step, while the ability is not cancelled.</summary>
    public interface IBattleEffect : IAbilityEffect
    {
        BattleStep Step { get; }

        void Apply(BattleCalculation calculation, PlayerId caster);
    }

    /// <summary>Changes the match at the moment the ability is played.</summary>
    public interface IPlayEffect : IAbilityEffect
    {
        /// <summary>Checks targets and preconditions without changing anything.</summary>
        RejectReason Validate(PlayContext context);

        void Apply(PlayContext context);
    }

    /// <summary>Forbids other abilities while the ability carrying it is in force in the battle.</summary>
    public interface IAbilityRestriction : IAbilityEffect
    {
        bool Forbids(AbilityCard ability, PlayerId player, PlayerId caster);
    }

    /// <summary>While in force, the field condition does not act at all.</summary>
    public interface IConditionCanceller : IAbilityEffect
    {
    }

    /// <summary>While in force, the field condition does not act on the caster's creature.</summary>
    public interface IConditionImmunity : IAbilityEffect
    {
    }
}
