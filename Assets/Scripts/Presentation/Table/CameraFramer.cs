using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>
    /// Keeps the whole table width on screen. On 16:9 the vertical field of view grows to fit the width;
    /// on wider screens (up to 21:9) the base view stays and the sides get more room.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFramer : MonoBehaviour
    {
        [SerializeField] private float _baseVerticalFov = 42f;
        [SerializeField] private float _tableHalfWidth = 8.6f;

        private Camera _camera;
        private float _aspect;

        private void Awake() => _camera = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (Mathf.Approximately(_aspect, _camera.aspect))
                return;

            _aspect = _camera.aspect;
            var distance = Mathf.Abs(transform.position.y / Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad));
            var halfHorizontal = Mathf.Atan(_tableHalfWidth / distance);
            var neededVertical = 2f * Mathf.Atan(Mathf.Tan(halfHorizontal) / _aspect) * Mathf.Rad2Deg;
            _camera.fieldOfView = Mathf.Max(_baseVerticalFov, neededVertical);
        }
    }
}
