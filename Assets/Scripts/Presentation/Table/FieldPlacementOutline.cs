using System;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>A non-interactive outline of the local player's empty field position.</summary>
    public sealed class FieldPlacementOutline : IDisposable
    {
        private readonly GameObject _root;
        private readonly UnityEngine.UI.Image[] _edges;

        public FieldPlacementOutline(TableLayout layout)
        {
            _root = new GameObject("FieldPlacementOutline", typeof(RectTransform), typeof(Canvas));
            _root.transform.SetParent(layout.transform, false);
            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = _root.GetComponent<RectTransform>();
            var pose = layout.Field(TableSide.Bottom, 0, 1, true);
            rect.position = pose.Position + Vector3.up * layout.FieldOutlineLift;
            rect.rotation = Quaternion.Euler(90f, 0f, 0f);
            var size = layout.CardSize * pose.Scale;
            rect.sizeDelta = size;
            var width = layout.FieldOutlineWidth;
            _edges = new[]
            {
                Edge("Top", new Vector2(0, size.y / 2), new Vector2(size.x + width, width)),
                Edge("Bottom", new Vector2(0, -size.y / 2), new Vector2(size.x + width, width)),
                Edge("Left", new Vector2(-size.x / 2, 0), new Vector2(width, size.y)),
                Edge("Right", new Vector2(size.x / 2, 0), new Vector2(width, size.y)),
            };
            _root.SetActive(false);
        }

        public void Show(bool visible, Color color)
        {
            if (_root == null) return;
            _root.SetActive(visible);
            if (visible) foreach (var edge in _edges) edge.color = color;
        }

        public void Dispose()
        {
            if (_root != null) UnityEngine.Object.Destroy(_root);
        }

        private UnityEngine.UI.Image Edge(string name, Vector2 position, Vector2 size)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            edge.transform.SetParent(_root.transform, false);
            var rect = edge.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = edge.GetComponent<UnityEngine.UI.Image>();
            image.raycastTarget = false;
            return image;
        }
    }
}
