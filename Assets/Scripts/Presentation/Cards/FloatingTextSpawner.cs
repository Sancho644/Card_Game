using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Short texts that rise above the table and fade, e.g. «+50 Кузница» over a creature.</summary>
    public sealed class FloatingTextSpawner
    {
        private const float FontSize = 3.2f;
        private const float Rise = 0.9f;

        private readonly Transform _root;
        private readonly Camera _camera;

        public FloatingTextSpawner(Transform root, Camera camera)
        {
            _root = root;
            _camera = camera;
        }

        /// <param name="offset">Lift above the anchor, to stack several texts.</param>
        public Tween Show(string text, Color color, Vector3 anchor, float offset, float duration)
        {
            var label = new GameObject("FloatingText").AddComponent<TextMeshPro>();
            label.transform.SetParent(_root, false);
            label.transform.SetPositionAndRotation(anchor + Vector3.up * (0.6f + offset), _camera.transform.rotation);
            label.text = text;
            label.color = color;
            label.fontSize = FontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.25f;
            label.outlineColor = Color.black;
            label.sortingOrder = 10;

            var start = label.transform.position;
            return DOTween.Sequence()
                .Append(label.transform.DOMove(start + _camera.transform.up * Rise, duration).SetEase(Ease.OutCubic))
                .Join(DOTween.To(() => label.alpha, alpha => label.alpha = alpha, 0f, duration * 0.4f).SetDelay(duration * 0.6f))
                .OnKill(() =>
                {
                    if (label != null)
                        Object.Destroy(label.gameObject);
                })
                .SetTarget(label);
        }
    }
}
