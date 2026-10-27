using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Keeps the table on screen. The camera stays where it is and only tilts and zooms: the smallest
    /// vertical field of view (not below the base one) and the tilt that put every framing point inside the
    /// safe area, below the top bar. On wide screens (up to 21:9) the base view stays and the sides get more
    /// room. Reports when the screen or the safe area changes, so the cards can be laid out again.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFramer : MonoBehaviour
    {
        private const float MaxVerticalFov = 90f;
        private const int SearchSteps = 24;

        [SerializeField] private float _baseVerticalFov = 42f;
        [SerializeField, Range(0f, 0.1f)] private float _screenMargin = 0.01f;

        private Camera _camera;
        private Func<IEnumerable<Vector3>> _points;
        private float _topBarShare;
        private Vector2Int _screen;
        private Rect _safeArea;

        public event Action Changed;

        public void Configure(Func<IEnumerable<Vector3>> points, float topBarShare)
        {
            _points = points;
            _topBarShare = topBarShare;
            Refit();
        }

        /// <summary>Fits the camera to the current screen at once, e.g. after rendering to a texture of another size.</summary>
        public void Refit()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();
            _screen = new Vector2Int(Screen.width, Screen.height);
            _safeArea = Screen.safeArea;
            if (_points != null)
                Fit(new List<Vector3>(_points()));
        }

        private void Awake() => _camera = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (_screen.x == Screen.width && _screen.y == Screen.height && _safeArea == Screen.safeArea)
                return;

            Refit();
            Changed?.Invoke();
        }

        private void Fit(IReadOnlyList<Vector3> points)
        {
            var area = AllowedViewport();
            if (TryFit(_baseVerticalFov, points, area))
                return;

            var low = _baseVerticalFov;
            var high = MaxVerticalFov;
            for (var i = 0; i < SearchSteps; i++)
            {
                var middle = (low + high) / 2f;
                if (TryFit(middle, points, area))
                    high = middle;
                else
                    low = middle;
            }

            TryFit(high, points, area);
        }

        /// <summary>
        /// Sets the field of view and the tilt that centres the points vertically in the area; true if they all fit.
        /// The camera only pitches, so a point's height on screen depends on its height and depth alone.
        /// </summary>
        private bool TryFit(float fov, IReadOnlyList<Vector3> points, Rect area)
        {
            var nearest = float.MinValue;
            var farthest = float.MaxValue;
            var origin = transform.position;
            foreach (var point in points)
            {
                // Angle below the horizon at which the camera sees the point.
                var below = Mathf.Atan2(origin.y - point.y, point.z - origin.z) * Mathf.Rad2Deg;
                nearest = Mathf.Max(nearest, below);
                farthest = Mathf.Min(farthest, below);
            }

            var halfTan = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            var bottomOffset = Mathf.Atan((0.5f - area.yMin) * 2f * halfTan) * Mathf.Rad2Deg;
            var topOffset = Mathf.Atan((0.5f - area.yMax) * 2f * halfTan) * Mathf.Rad2Deg;
            var minPitch = nearest - bottomOffset;
            var maxPitch = farthest - topOffset;

            _camera.fieldOfView = fov;
            transform.rotation = Quaternion.Euler((minPitch + maxPitch) / 2f, 0f, 0f);
            if (minPitch > maxPitch)
                return false;

            foreach (var point in points)
            {
                var viewport = _camera.WorldToViewportPoint(point);
                if (viewport.z <= 0f || viewport.x < area.xMin || viewport.x > area.xMax)
                    return false;
            }

            return true;
        }

        private Rect AllowedViewport()
        {
            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            var safe = Screen.safeArea;
            var top = Mathf.Min(safe.yMax / height, 1f - _topBarShare);
            return Rect.MinMaxRect(
                safe.xMin / width + _screenMargin,
                safe.yMin / height + _screenMargin,
                safe.xMax / width - _screenMargin,
                top - _screenMargin);
        }
    }
}
