using System;
using System.Collections.Generic;
using Game.Core.Commands;
using Game.Core.State;
using Game.Services.Match;

namespace Game.Presentation.Interaction
{
    /// <summary>A player sitting at this screen: decisions are made through the table and the interface.</summary>
    public sealed class LocalHumanController : IPlayerController
    {
        private Action<GameCommand> _submit;

        public LocalHumanController(PlayerId player)
        {
            Player = player;
        }

        /// <summary>The interface should offer these actions to the player.</summary>
        public event Action<LocalHumanController, PlayerView, IReadOnlyList<GameCommand>> DecisionRequested;

        /// <summary>The interface may offer these optional actions while the opponent is deciding.</summary>
        public event Action<LocalHumanController, PlayerView, IReadOnlyList<GameCommand>> ReactionOffered;

        public event Action<LocalHumanController> DecisionCancelled;

        public PlayerId Player { get; }

        public bool IsDeciding => _submit != null;

        public void RequestDecision(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit)
        {
            _submit = submit;
            DecisionRequested?.Invoke(this, view, legalActions);
        }

        public void OfferReaction(PlayerView view, IReadOnlyList<GameCommand> legalActions, Action<GameCommand> submit)
        {
            _submit = submit;
            ReactionOffered?.Invoke(this, view, legalActions);
        }

        public void CancelDecision()
        {
            if (_submit == null)
                return;
            _submit = null;
            DecisionCancelled?.Invoke(this);
        }

        public void AbandonDecision() => _submit = null;

        /// <summary>Sends the player's choice. Only commands from the offered list should come here.</summary>
        public void Submit(GameCommand command)
        {
            var submit = _submit;
            _submit = null;
            submit?.Invoke(command);
        }
    }
}
