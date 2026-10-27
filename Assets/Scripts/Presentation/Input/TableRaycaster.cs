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

        /// <summary>The point under the screen position on a horizontal plane at the given height.</summary>
        public Vector3 PointAtHeight(Vector2 screenPosition, float height)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : ray.GetPoint(10f);
        }
    }
}
