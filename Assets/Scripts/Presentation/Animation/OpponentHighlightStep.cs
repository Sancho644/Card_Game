using System;
using DG.Tweening;
using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Animation
{
    /// <summary>Highlights the already visible card during a scalable pause; a missing card still gets the pause.</summary>
    public sealed class OpponentHighlightStep : IAnimationStep
    {
        private readonly Func<CardView> _findCard;
        private readonly Color _color;
        private readonly float _seconds;
        private CardView _card;
        private Tween _delay;

        public OpponentHighlightStep(Func<CardView> findCard, Color color, float seconds)
        {
            _findCard = findCard;
            _color = color;
            _seconds = seconds;
        }

        public void Play(Action done)
        {
            _card = _findCard();
            if (_card != null && _card.IsKnown) _card.SetHighlight(_color);
            // DOTween.timeScale is controlled by AnimationSpeed, including fast animations and tap acceleration.
            _delay = DOVirtual.DelayedCall(_seconds, () => { ClearHighlight(); done(); });
        }

        public void Complete()
        {
            _delay?.Kill();
            ClearHighlight();
        }

        private void ClearHighlight()
        {
            if (_card != null) _card.SetHighlight(null);
        }
    }
}
