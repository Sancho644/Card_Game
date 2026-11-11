using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>A live progress arc and a moving light; no textures or per-frame allocations.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LoadingRingGraphic : UnityEngine.UI.MaskableGraphic
    {
        private float _progress;
        private float _phase;

        public void Show(float progress, float phase)
        {
            _progress = Mathf.Clamp01(progress);
            _phase = Mathf.Repeat(phase, 1f);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .469f;
            var width = radius * .012f;
            Arc(mesh, radius, width, 0, 1, new Color(.5f, .38f, .19f, .55f));
            Arc(mesh, radius, width * 3.5f, 0, _progress, new Color(color.r, color.g, color.b, .09f));
            Arc(mesh, radius, width, 0, _progress, color);
            // The independent moving glint keeps responding when the scene loader waits for activation.
            for (var i = 0; i < 12; i++)
            {
                var end = Mathf.Repeat(_phase - i * .004f, 1);
                var tint = Color.Lerp(color, Color.white, .65f);
                tint.a = (1f - i / 12f) * .85f;
                Arc(mesh, radius, width * 1.7f, end, .004f, tint);
            }
        }

        private void Arc(UnityEngine.UI.VertexHelper mesh, float radius, float width, float start, float length, Color tint)
        {
            var count = Mathf.CeilToInt(length * 144);
            var centre = rectTransform.rect.center;
            for (var i = 0; i < count; i++)
            {
                var a = (90f - (start + length * i / count) * 360f) * Mathf.Deg2Rad;
                var b = (90f - (start + length * (i + 1) / count) * 360f) * Mathf.Deg2Rad;
                var first = mesh.currentVertCount;
                var v = UnityEngine.UIVertex.simpleVert;
                v.color = tint;
                v.position = centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (radius - width); mesh.AddVert(v);
                v.position = centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (radius + width); mesh.AddVert(v);
                v.position = centre + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * (radius + width); mesh.AddVert(v);
                v.position = centre + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * (radius - width); mesh.AddVert(v);
                mesh.AddTriangle(first, first + 1, first + 2);
                mesh.AddTriangle(first, first + 2, first + 3);
            }
        }
    }
}
