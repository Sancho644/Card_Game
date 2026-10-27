using System;
using Game.Core.State;

namespace Game.Core.Battle
{
    /// <summary>A difference below the margin is a draw; otherwise the inner rule decides.</summary>
    public sealed class NarrowMarginDraw : IBattleComparison
    {
        private readonly IBattleComparison _inner;
        private readonly int _margin;

        public NarrowMarginDraw(IBattleComparison inner, int margin)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _margin = margin;
        }

        public PlayerId? DecideWinner(BattleCalculation calculation)
        {
            var difference = Math.Abs(calculation.PowerOf(calculation.Host) - calculation.PowerOf(calculation.Guest));
            return difference < _margin ? null : _inner.DecideWinner(calculation);
        }
    }
}
