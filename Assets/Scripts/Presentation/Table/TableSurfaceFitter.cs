using Game.Data.Presentation;
using UnityEngine;

namespace Game.Presentation.Table
{
    /// <summary>Extends the centered table under the whole camera view without moving the camera or cards.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class TableSurfaceFitter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TablePresentationSettings _settings;
        [SerializeField] private MeshFilter _surface;
        [SerializeField] private Transform _sides;

        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int LineWidth = Shader.PropertyToID("_LineWidth");
        private static readonly Vector2[] Corners = { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };

        private Vector3 _surfaceScale;
        private Vector3 _sidesScale;
        private Renderer _surfaceRenderer;
        private Renderer _sidesRenderer;
        private MaterialPropertyBlock _surfaceBlock;
        private MaterialPropertyBlock _sidesBlock;
        private Vector2 _textureScale;
        private Vector2 _textureOffset;
        private float _lineWidth;
        private Matrix4x4 _appliedCamera;
        private float _appliedPadding = -1;

        private void Awake()
        {
            _surfaceScale = _surface.transform.localScale;
            _sidesScale = _sides.localScale;
            _surfaceRenderer = _surface.GetComponent<Renderer>();
            _sidesRenderer = _sides.GetComponent<Renderer>();
            _surfaceBlock = new MaterialPropertyBlock();
            _sidesBlock = new MaterialPropertyBlock();
            _textureScale = _surfaceRenderer.sharedMaterial.GetTextureScale("_BaseMap");
            _textureOffset = _surfaceRenderer.sharedMaterial.GetTextureOffset("_BaseMap");
            _lineWidth = _sidesRenderer.sharedMaterial.GetFloat(LineWidth);
            Fit();
        }

        private void LateUpdate() => Fit();

        public void Fit()
        {
            var cameraMatrix = _camera.projectionMatrix * _camera.worldToCameraMatrix;
            var padding = _settings.TableScreenPadding;
            if (_appliedCamera == cameraMatrix && Mathf.Approximately(_appliedPadding, padding)) return;
            _appliedCamera = cameraMatrix; _appliedPadding = padding;

            var surface = _surface.transform;
            var parent = surface.parent != null ? surface.parent.localToWorldMatrix : Matrix4x4.identity;
            var baseline = parent * Matrix4x4.TRS(surface.localPosition, surface.localRotation, _surfaceScale);
            var inverse = baseline.inverse;
            var bounds = _surface.sharedMesh.bounds;
            var plane = new Plane(surface.up, baseline.MultiplyPoint3x4(bounds.center));
            var padX = padding / baseline.MultiplyVector(Vector3.right).magnitude;
            var padZ = padding / baseline.MultiplyVector(Vector3.forward).magnitude;
            var factor = 1f;
            foreach (var corner in Corners)
            {
                var ray = _camera.ViewportPointToRay(new Vector3(corner.x, corner.y, 0));
                if (!plane.Raycast(ray, out var distance)) continue;
                var point = inverse.MultiplyPoint3x4(ray.GetPoint(distance)) - bounds.center;
                factor = Mathf.Max(factor, (Mathf.Abs(point.x) + padX) / bounds.extents.x,
                    (Mathf.Abs(point.z) + padZ) / bounds.extents.z);
            }

            surface.localScale = new Vector3(_surfaceScale.x * factor, _surfaceScale.y, _surfaceScale.z * factor);
            _sides.localScale = new Vector3(_sidesScale.x * factor, _sidesScale.y, _sidesScale.z * factor);
            // Keep the same texel density and texture phase at every world point as the mesh grows.
            _surfaceRenderer.GetPropertyBlock(_surfaceBlock);
            _surfaceBlock.SetVector(BaseMapSt, new Vector4(_textureScale.x * factor, _textureScale.y * factor,
                _textureOffset.x + _textureScale.x * (1 - factor) * .5f,
                _textureOffset.y + _textureScale.y * (1 - factor) * .5f));
            _surfaceRenderer.SetPropertyBlock(_surfaceBlock);
            // Scaling the overlay must not make its central divider thicker in world space.
            _sidesRenderer.GetPropertyBlock(_sidesBlock);
            _sidesBlock.SetFloat(LineWidth, _lineWidth / factor);
            _sidesRenderer.SetPropertyBlock(_sidesBlock);
        }
    }
}
