using UnityEngine;

namespace Game.Presentation.Interaction
{
    /// <summary>A card a dragged card may land on, with the screen area it covers.</summary>
    public readonly struct DropCandidate
    {
        public readonly int InstanceId;
        public readonly Rect ScreenRect;

        public DropCandidate(int instanceId, Rect screenRect)
        {
            InstanceId = instanceId;
            ScreenRect = screenRect;
        }
    }
}
