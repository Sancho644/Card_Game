using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>
    /// Chooses where a dragged card lands: the target whose screen area is nearest to the release point,
    /// if it is within the radius. Areas are enlarged by a padding, so the drop zone is larger than the card;
    /// where padded areas overlap, the nearer centre wins.
    /// </summary>
    public sealed class DropTargetPicker
    {
        /// <param name="padding">Pixels added on every side of each target's area.</param>
        /// <param name="radius">Pixels beyond the padded area that still count as a drop on the target.</param>
        public int? Pick(Vector2 point, IEnumerable<DropCandidate> candidates, float padding, float radius)
        {
            int? best = null;
            var bestDistance = float.MaxValue;
            var bestCentre = float.MaxValue;
            foreach (var candidate in candidates)
            {
                var distance = DistanceToRect(point, candidate.ScreenRect, padding);
                if (distance > radius)
                    continue;

                // Overlapping areas: the card whose centre is nearer wins.
                var centre = (candidate.ScreenRect.center - point).magnitude;
                if (distance > bestDistance || (Mathf.Approximately(distance, bestDistance) && centre >= bestCentre))
                    continue;
                best = candidate.InstanceId;
                bestDistance = distance;
                bestCentre = centre;
            }

            return best;
        }

        /// <summary>0 inside the padded rect, otherwise the distance to its nearest edge.</summary>
        private static float DistanceToRect(Vector2 point, Rect rect, float padding)
        {
            var dx = Mathf.Max(rect.xMin - padding - point.x, 0f, point.x - rect.xMax - padding);
            var dy = Mathf.Max(rect.yMin - padding - point.y, 0f, point.y - rect.yMax - padding);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
