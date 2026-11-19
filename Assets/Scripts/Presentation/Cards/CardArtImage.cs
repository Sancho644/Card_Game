using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Cards
{
    /// <summary>Blends the edges of a fitted illustration into its border bleed.</summary>
    public sealed class CardArtImage : RawImage
    {
        private const float Feather = .11f;

        public bool FeatherHorizontally { get; set; }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            var uv = uvRect;
            var tint = (Color32)color;
            for (var i = 0; i < 4; i++)
            {
                var along = i == 0 ? 0f : i == 1 ? Feather : i == 2 ? 1f - Feather : 1f;
                var shade = tint;
                if (i == 0 || i == 3) shade.a = 0;
                for (var edge = 0; edge < 2; edge++)
                {
                    var x = FeatherHorizontally ? along : edge;
                    var y = FeatherHorizontally ? edge : along;
                    var point = new Vector3(rect.x + x * rect.width, rect.y + y * rect.height, 0);
                    var sample = new Vector2(uv.x + x * uv.width, uv.y + y * uv.height);
                    mesh.AddVert(point, shade, sample);
                }
                if (i == 0) continue;
                var first = (i - 1) * 2;
                if (FeatherHorizontally)
                {
                    mesh.AddTriangle(first, first + 1, first + 3);
                    mesh.AddTriangle(first + 3, first + 2, first);
                }
                else
                {
                    mesh.AddTriangle(first, first + 2, first + 3);
                    mesh.AddTriangle(first + 3, first + 1, first);
                }
            }
        }
    }
}
