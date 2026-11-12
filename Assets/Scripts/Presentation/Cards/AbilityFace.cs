namespace Game.Presentation.Cards
{
    /// <summary>Presentation data for a spell; its bonus is separate from a creature's base power.</summary>
    public sealed class AbilityFace
    {
        public AbilityFace(int? bonus, string bonusSuffix, string timing, string rules)
        {
            Bonus = bonus;
            BonusSuffix = bonusSuffix;
            Timing = timing;
            Rules = rules;
        }

        public int? Bonus { get; }
        public string BonusSuffix { get; }
        public string Timing { get; }
        public string Rules { get; }
    }
}
