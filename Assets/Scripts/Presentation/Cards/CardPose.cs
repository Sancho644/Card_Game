using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>Where a card lies and how it is turned; face up or down is part of the rotation.</summary>
    public readonly struct CardPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public CardPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
