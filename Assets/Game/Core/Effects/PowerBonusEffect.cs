using System.Collections.Generic;
using System.Linq;
using Game.Core.Battle;
using Game.Core.State;

namespace Game.Core.Effects
{
    /// <summary>"+N to your creature's power", optionally only when every requirement is met.</summary>
    public sealed class PowerBonusEffect : IBattleEffect
    {
        private readonly int _amount;
        private readonly IReadOnlyList<IEffectRequirement> _requirements;

        public PowerBonusEffect(int amount, params IEffectRequirement[] requirements)
        {
            _amount = amount;
            _requirements = requirements ?? new IEffectRequirement[0];
        }

        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster)
        {
            if (_requirements.All(requirement => requirement.IsMet(calculation, caster)))
                calculation.AddPower(caster, _amount);
        }
    }

    /// <summary>"−N to the opponent's creature's power".</summary>
    public sealed class OpponentPowerEffect : IBattleEffect
    {
        private readonly int _amount;

        public OpponentPowerEffect(int amount)
        {
            _amount = amount;
        }

        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.AddPower(caster.Opponent(), _amount);
    }

    /// <summary>On equal final power, your creature wins.</summary>
    public sealed class WinTiesEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) => calculation.TieWinner = caster;
    }

    /// <summary>If your creature loses this battle, it returns to hand instead of going out of the game.</summary>
    public sealed class ReturnToHandIfLostEffect : IBattleEffect
    {
        public BattleStep Step => BattleStep.Power;

        public void Apply(BattleCalculation calculation, PlayerId caster) =>
            calculation.ModifyConsequences(consequences =>
            {
                if (consequences.Loser == caster && consequences.FateOf(caster) == CreatureFate.OutOfGame)
                    consequences.SetFate(caster, CreatureFate.ReturnsToHand);
            });
    }

    /// <summary>The open field's condition stops working; bonuses and penalties stay.</summary>
    public sealed class CancelFieldConditionEffect : IConditionCanceller
    {
    }

    /// <summary>The open field's condition does not act on your creature.</summary>
    public sealed class ConditionImmunityEffect : IConditionImmunity
    {
    }
}
