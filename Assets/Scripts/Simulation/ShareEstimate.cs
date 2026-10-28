using System;

namespace Game.Simulation
{
    /// <summary>A share of successes with its 95% Wilson confidence interval.</summary>
    public sealed class ShareEstimate
    {
        private const double Z = 1.96;

        public ShareEstimate(int successes, int trials)
        {
            Successes = successes;
            Trials = trials;
            if (trials == 0)
                return;

            var share = (double)successes / trials;
            var z2 = Z * Z;
            var denominator = 1 + z2 / trials;
            var centre = (share + z2 / (2 * trials)) / denominator;
            var spread = Z * Math.Sqrt(share * (1 - share) / trials + z2 / (4.0 * trials * trials)) / denominator;
            Low = Math.Max(0, centre - spread);
            High = Math.Min(1, centre + spread);
        }

        public int Successes { get; }
        public int Trials { get; }
        public double Share => Trials == 0 ? 0 : (double)Successes / Trials;
        public double Low { get; }
        public double High { get; }

        /// <summary>The interval lies wholly above or below one half: the share differs from 50% with 95% confidence.</summary>
        public bool ExcludesHalf => Trials > 0 && (Low > 0.5 || High < 0.5);
    }
}
