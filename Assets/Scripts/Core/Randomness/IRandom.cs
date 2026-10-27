namespace Game.Core.Randomness
{
    /// <summary>The engine's only source of randomness. Passed in from outside so matches are reproducible.</summary>
    public interface IRandom
    {
        /// <summary>Returns a value in [0, maxExclusive).</summary>
        int Next(int maxExclusive);
    }
}
