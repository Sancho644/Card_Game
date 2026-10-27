using System;

namespace Game.Data.Effects
{
    /// <summary>Name shown in the inspector's "add effect" menu. Designer-facing, not player-facing.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DataLabelAttribute : Attribute
    {
        public DataLabelAttribute(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
