using System;
using System.Collections.Generic;
using Game.Data.Presentation;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Places the camera from the table settings: tilt, height and angle of view. The camera slides along
    /// the table so the framing points sit in the middle of the safe area below the top bar; if they still do
    /// not fit, the angle of view grows until they do. On wide screens (up to 21:9) the sides simply get more
    /// room. Follows changes of the screen,
    /// the safe area and the settings themselves (they can be tuned in the inspector in Play mode) and
    /// reports them, so the cards can be laid out again.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFramer : MonoBehaviour
    {
        private const float MaxVerticalFov = 90f;
        private const int SearchSteps = 24;

        [SerializeField, Range(0f, 0.1f)] private float _screenMargin = 0.01f;

        private Camera _camera;
        private TablePresentationSettings _settings;
        private Func<IEnumerable<Vector3>> _points;
        private float _topBarShare;
        private Vector2Int _screen;
        private Rect _safeArea;
        private Vector3 _applied;

        public event Action Changed;

        public void Configure(TablePresentationSettings settings, Func<IEnumerable<Vector3>> points)
        {
            _settings = settings;
            _points = points;
            _topBarShare = settings.TopBarShare;
            Refit();
        }

        /// <summary>Fits the camera to the current screen and settings at once.</summary>
        public void Refit()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();
            _screen = new Vector2Int(Screen.width, Screen.height);
            _safeArea = Screen.safeArea;
            if (_settings == null)
                return;

            _applied = Snapshot();
            var points = _points != null ? new List<Vector3>(_points()) : new List<Vector3>();
            FitAngleOfView(points);
        }

        private void Awake() => _camera = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (_settings == null)
                return;
            if (_screen.x == Screen.width && _screen.y == Screen.height && _safeArea == Screen.safeArea && _applied == Snapshot())
                return;

            Refit();
            Changed?.Invoke();
        }

        private Vector3 Snapshot() =>
            new Vector3(_settings.CameraPitch, _settings.CameraHeight, _settings.CameraFieldOfView);

        /// <summary>Above the table at the set height and tilt, slid along the table to <paramref name="z"/>.</summary>
        private void Place(float z) =>
            transform.SetPositionAndRotation(new Vector3(0f, _settings.CameraHeight, z), Quaternion.Euler(_settings.CameraPitch, 0f, 0f));

        private void FitAngleOfView(IReadOnlyList<Vector3> points)
        {
            var area = AllowedViewport();
            var start = _settings.CameraFieldOfView;
            if (Fits(start, points, area))
                return;

            var low = start;
            var high = MaxVerticalFov;
            for (var i = 0; i < SearchSteps; i++)
            {
                var middle = (low + high) / 2f;
                if (Fits(middle, points, area))
                    high = middle;
                else
                    low = middle;
            }

            Fits(high, points, area);
        }

        /// <summary>Sets the angle of view, centres the points vertically; true if every point is then inside the area.</summary>
        private bool Fits(float fov, IReadOnlyList<Vector3> points, Rect area)
        {
            _camera.fieldOfView = fov;
            Centre(points, area);
            foreach (var point in points)
            {
                var viewport = _camera.WorldToViewportPoint(point);
                if (viewport.z <= 0f || !area.Contains(viewport))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Slides the camera along the table until the highest and lowest points are equally far from the
        /// area's edges. Stepping back moves every point up the screen, so a bisection finds the place.
        /// </summary>
        private void Centre(IReadOnlyList<Vector3> points, Rect area)
        {
            var near = -2f * _settings.CameraHeight - 20f;
            var far = 20f;
            var middle = 0f;
            for (var i = 0; i < SearchSteps; i++)
            {
                middle = (near + far) / 2f;
                Place(middle);
                if (points.Count == 0)
                    return;

                var low = float.MaxValue;
                var high = float.MinValue;
                foreach (var point in points)
                {
                    var y = _camera.WorldToViewportPoint(point).y;
                    low = Mathf.Min(low, y);
                    high = Mathf.Max(high, y);
                }

                // Points too high: come forward; too low: step back.
                if ((low + high) / 2f > area.center.y)
                    near = middle;
                else
                    far = middle;
            }
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
