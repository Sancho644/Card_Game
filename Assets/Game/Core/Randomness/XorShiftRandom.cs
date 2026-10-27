using System;

namespace Game.Core.Randomness
{
    /// <summary>
    /// Seeded xorshift64* generator. Implemented here instead of System.Random so the sequence
    /// is identical on any runtime (client, server).
    /// </summary>
    public sealed class XorShiftRandom : IRandom
    {
        private ulong _state;

        public XorShiftRandom(ulong seed)
        {
            // splitmix64 spreads small seeds (1, 2, 3...) over the whole state; xorshift needs a non-zero state.
            var z = seed + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            _state = z ^ (z >> 31);
            if (_state == 0)
                _state = 0x9E3779B97F4A7C15UL;
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            _state ^= _state >> 12;
            _state ^= _state << 25;
            _state ^= _state >> 27;
            var value = _state * 0x2545F4914F6CDD1DUL;
            return (int)((value >> 33) % (ulong)maxExclusive);
        }
    }
}
