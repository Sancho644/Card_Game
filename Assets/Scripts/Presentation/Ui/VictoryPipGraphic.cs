using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>A small victory jewel drawn as UI geometry, without font glyphs or textures.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class VictoryPipGraphic : UnityEngine.UI.MaskableGraphic
    {
        private bool _filled;
        private Color _lit;

        public void Show(bool filled, Color lit)
        {
            if (_filled == filled && _lit == lit) return;
            _filled = filled;
            _lit = lit;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .5f;
            Ring(mesh, radius * .82f, radius, new Color(.38f,.22f,.08f), new Color(.9f,.7f,.38f));
            Ring(mesh, radius * .70f, radius * .82f, new Color(.04f,.09f,.09f), new Color(.2f,.13f,.05f));
            var edge = _filled ? _lit * .65f : new Color(.02f,.045f,.045f);
            var centre = _filled ? Color.Lerp(_lit, Color.white, .35f) : new Color(.04f,.085f,.085f);
            edge.a = centre.a = 1;
            Ring(mesh, 0, radius * .70f, centre, edge);
        }

        private void Ring(UnityEngine.UI.VertexHelper mesh, float inner, float outer, Color inside, Color outside)
        {
            const int segments = 32;
            var centre = rectTransform.rect.center;
            for (var i = 0; i < segments; i++)
            {
                var a = i * Mathf.PI * 2 / segments;
                var b = (i + 1) * Mathf.PI * 2 / segments;
                var first = mesh.currentVertCount;
                var v = UnityEngine.UIVertex.simpleVert;
                v.color = inside; v.position = centre + new Vector2(Mathf.Cos(a),Mathf.Sin(a)) * inner; mesh.AddVert(v);
                v.position = centre + new Vector2(Mathf.Cos(b),Mathf.Sin(b)) * inner; mesh.AddVert(v);
                v.color = outside; v.position = centre + new Vector2(Mathf.Cos(b),Mathf.Sin(b)) * outer; mesh.AddVert(v);
                v.position = centre + new Vector2(Mathf.Cos(a),Mathf.Sin(a)) * outer; mesh.AddVert(v);
                mesh.AddTriangle(first, first+1, first+2);
                mesh.AddTriangle(first, first+2, first+3);
            }
        }
    }
}
