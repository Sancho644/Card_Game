using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>Three travelling lights, drawn with a small UI mesh and no extra textures.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SearchPulseGraphic : UnityEngine.UI.MaskableGraphic
    {
        private float _phase;

        public void Show(float phase)
        {
            _phase = Mathf.Repeat(phase, 1);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            var radius = Mathf.Min(rect.height * .10f, rect.width * .025f);
            for (var i = 0; i < 3; i++)
            {
                var t = Mathf.Repeat(_phase + i / 3f, 1);
                var alpha = Mathf.Sin(t * Mathf.PI);
                var point = new Vector2(Mathf.Lerp(rect.xMin + radius * 4, rect.xMax - radius * 4, t), rect.center.y);
                Disc(mesh, point, radius * 3.8f, new Color(color.r, color.g, color.b, alpha * .25f));
                Disc(mesh, point, radius * 1.4f, new Color(color.r, color.g, color.b, alpha));
                Disc(mesh, point, radius * .65f, new Color(1, 1, 1, alpha));
            }
        }

        private static void Disc(UnityEngine.UI.VertexHelper mesh, Vector2 centre, float radius, Color tint)
        {
            var first = mesh.currentVertCount;
            var v = UnityEngine.UIVertex.simpleVert;
            v.position = centre; v.color = tint; mesh.AddVert(v);
            const int segments = 24;
            for (var i = 0; i <= segments; i++)
            {
                var angle = i * Mathf.PI * 2 / segments;
                v.position = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                v.color = new Color(tint.r, tint.g, tint.b, 0); mesh.AddVert(v);
                if (i > 0) mesh.AddTriangle(first, first + i, first + i + 1);
            }
        }
    }
}
