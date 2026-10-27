using Game.Presentation.Cards;
using UnityEngine;

namespace Game.Presentation.Input
{
    /// <summary>Finds what lies under a screen point on the 3D table.</summary>
    public sealed class TableRaycaster
    {
        private readonly Camera _camera;

        public TableRaycaster(Camera camera)
        {
            _camera = camera;
        }

        public CardView CardAt(Vector2 screenPosition, CardView ignore = null)
        {
            var hits = Physics.RaycastAll(_camera.ScreenPointToRay(screenPosition), 100f);
            CardView nearest = null;
            var nearestDistance = float.MaxValue;
            foreach (var hit in hits)
            {
                var card = hit.collider.GetComponentInParent<CardView>();
                if (card == null || card == ignore || hit.distance >= nearestDistance)
                    continue;
                nearest = card;
                nearestDistance = hit.distance;
            }

            return nearest;
        }

        /// <summary>The screen area a card covers: its collider's bounds projected to the screen, in pixels.</summary>
        public Rect ScreenRectOf(CardView card)
        {
            var bounds = card.Collider.bounds;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (var corner = 0; corner < 8; corner++)
            {
                var point = _camera.WorldToScreenPoint(new Vector3(
                    (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z));
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>The point under the screen position on a horizontal plane at the given height.</summary>
        public Vector3 PointAtHeight(Vector2 screenPosition, float height)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : ray.GetPoint(10f);
        }
    }
}
