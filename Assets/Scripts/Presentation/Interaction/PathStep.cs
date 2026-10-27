namespace Game.Presentation.Interaction
{
    public readonly struct PathStep
    {
        public readonly PathStepKind Kind;

        /// <summary>A card instance id, or <see cref="ActionPath.NoOption"/> for "without an ability".</summary>
        public readonly int Value;

        public PathStep(PathStepKind kind, int value)
        {
            Kind = kind;
            Value = value;
        }
    }
}
