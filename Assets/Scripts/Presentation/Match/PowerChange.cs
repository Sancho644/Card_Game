namespace Game.Presentation.Match
{
    /// <summary>How much one source (the field or an ability) changed a creature's power since it was last shown.</summary>
    public readonly struct PowerChange
    {
        public readonly string Source;
        public readonly int Amount;

        public PowerChange(string source, int amount)
        {
            Source = source;
            Amount = amount;
        }
    }
}
