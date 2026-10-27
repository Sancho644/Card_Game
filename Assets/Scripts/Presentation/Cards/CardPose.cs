using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Where a card lies, how it is turned and how large it is; face up or down is part of the rotation.</summary>
    public readonly struct CardPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float Scale;

        public CardPose(Vector3 position, Quaternion rotation, float scale = 1f)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
