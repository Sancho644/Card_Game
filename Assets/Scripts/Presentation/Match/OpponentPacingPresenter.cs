using System;
using System.Collections.Generic;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Presentation;
using Game.Presentation.Animation;

namespace Game.Presentation.Match
{
    /// <summary>One presentation pause per remote action, after its visible consequences. Hot-seat has no extra pause.</summary>
    public sealed class OpponentPacingPresenter
    {
        private readonly MatchMode _mode;
        private readonly AnimationSettings _settings;
        private readonly Func<int?, float, IAnimationStep> _createStep;

        public OpponentPacingPresenter(MatchMode mode, AnimationSettings settings, Func<int?, float, IAnimationStep> createStep)
        {
            _mode = mode;
            _settings = settings;
            _createStep = createStep;
        }

        public IAnimationStep CreateStep(IReadOnlyList<GameEvent> batch, PlayerId viewer, Func<int, PlayerId?> fieldOwner)
        {
            if (_mode.ShowsHandoff || _settings.OpponentActionSeconds <= 0f) return null;
            var cue = OpponentActionCue.FromBatch(batch, fieldOwner);
            return cue == null || cue.Player == viewer ? null : _createStep(cue.CardId, _settings.OpponentActionSeconds);
        }
    }
}
