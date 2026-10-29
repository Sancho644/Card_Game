using UnityEngine;

namespace Game.Data.Editor
{
    /// <summary>
    /// Simple geometric shapes for placeholder art, in the unit square (x right, y up). Each returns the
    /// brightness of the point: 0 outside, up to 1 inside; shaded shapes return values in between.
    /// </summary>
    public static class PlaceholderShapes
    {
        /// <summary>Пламя: a tall tongue of fire with two small ones at its sides.</summary>
        public static float Fire(float x, float y) =>
            Tongue(x, y, 0.5f, 0.36f, 0.21f, 0.58f) > 0f || Tongue(x, y, 0.28f, 0.30f, 0.12f, 0.34f) > 0f || Tongue(x, y, 0.72f, 0.30f, 0.12f, 0.34f) > 0f ? 1f : 0f;

        /// <summary>Капля: one smooth drop pointing up.</summary>
        public static float Water(float x, float y) => Tongue(x, y, 0.5f, 0.36f, 0.26f, 0.56f, 1.4f);

        /// <summary>Камень: an uneven rock.</summary>
        public static float Earth(float x, float y)
        {
            var rock = new[]
            {
                new Vector2(0.16f, 0.32f), new Vector2(0.34f, 0.12f), new Vector2(0.70f, 0.10f), new Vector2(0.88f, 0.36f),
                new Vector2(0.80f, 0.74f), new Vector2(0.50f, 0.90f), new Vector2(0.18f, 0.70f),
            };
            return Inside(rock, new Vector2(x, y)) ? 1f : 0f;
        }

        /// <summary>Вихрь: a spiral of three turns.</summary>
        public static float Wind(float x, float y)
        {
            var dx = x - 0.5f;
            var dy = y - 0.5f;
            var radius = Mathf.Sqrt(dx * dx + dy * dy);
            var angle = Mathf.Atan2(dy, dx);
            if (angle < 0f)
                angle += 2f * Mathf.PI;
            for (var turn = 0; turn < 3; turn++)
            {
                var phi = angle + 2f * Mathf.PI * turn;
                if (phi > 7.1f)
                    break;
                var spiral = 0.05f + 0.054f * phi;
                if (Mathf.Abs(radius - spiral) < 0.045f)
                    return 1f;
            }

            return radius < 0.07f ? 1f : 0f;
        }

        /// <summary>Луч: a sun with eight rays.</summary>
        public static float Light(float x, float y)
        {
            var dx = x - 0.5f;
            var dy = y - 0.5f;
            var radius = Mathf.Sqrt(dx * dx + dy * dy);
            if (radius < 0.2f)
                return 1f;
            if (radius < 0.26f || radius > 0.47f)
                return 0f;

            var step = Mathf.PI / 4f;
            var angle = Mathf.Atan2(dy, dx);
            var offset = Mathf.Abs(Mathf.Repeat(angle + step / 2f, step) - step / 2f);
            var halfWidth = 0.075f * (0.47f - radius) / 0.21f;
            return offset * radius < halfWidth ? 1f : 0f;
        }

        /// <summary>Полумесяц: a crescent moon.</summary>
        public static float Dark(float x, float y) =>
            Circle(x, y, 0.47f, 0.5f, 0.36f) && !Circle(x, y, 0.62f, 0.60f, 0.30f) ? 1f : 0f;

        /// <summary>A cut gem with lighter and darker facets, to be tinted with the rarity color.</summary>
        public static float Gem(float x, float y)
        {
            var dx = (x - 0.5f) / 0.42f;
            var dy = (y - 0.5f) / 0.47f;
            if (Mathf.Abs(dx) + Mathf.Abs(dy) > 1f)
                return 0f;
            if (Mathf.Abs(dx) + Mathf.Abs(dy) < 0.45f)
                return 1f;
            if (dy >= 0f)
                return dx <= 0f ? 0.92f : 0.78f;
            return dx <= 0f ? 0.66f : 0.52f;
        }

        /// <summary>The card back: a lattice, a double border and a diamond emblem; opaque.</summary>
        public static float Back(float x, float y)
        {
            var edge = Mathf.Min(Mathf.Min(x, 1f - x), Mathf.Min(y, 1f - y) * 1.43f);
            if (edge < 0.035f)
                return 1f;
            if (edge > 0.055f && edge < 0.07f)
                return 1f;

            var diamond = Mathf.Abs(x - 0.5f) / 0.26f + Mathf.Abs(y - 0.5f) / 0.18f;
            if (diamond < 1f && diamond > 0.86f)
                return 1f;
            if (diamond < 0.5f)
                return 0.95f;

            var lattice = Mathf.Abs(Mathf.Repeat(x * 8f + y * 5.6f, 1f) - 0.5f) < 0.06f || Mathf.Abs(Mathf.Repeat(x * 8f - y * 5.6f, 1f) - 0.5f) < 0.06f;
            return lattice ? 0.82f : 0.7f;
        }

        /// <summary>A teardrop pointing up: round below its base, narrowing to a point above.</summary>
        private static float Tongue(float x, float y, float centerX, float baseY, float width, float height, float sharpness = 0.8f)
        {
            if (y <= baseY)
                return Circle(x, y, centerX, baseY, width) ? 1f : 0f;
            var t = (y - baseY) / height;
            if (t >= 1f)
                return 0f;
            return Mathf.Abs(x - centerX) < width * Mathf.Pow(1f - t, sharpness) ? 1f : 0f;
        }

        private static bool Circle(float x, float y, float centerX, float centerY, float radius) =>
            (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) < radius * radius;

        private static bool Inside(Vector2[] polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                    inside = !inside;
            }

            return inside;
        }
    }
}
