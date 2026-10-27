using System;
using UnityEngine;

namespace Game.Data.Config
{
    [Serializable]
    public struct IntRange
    {
        public int Min;
        public int Max;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int value) => value >= Min && value <= Max;

        public override string ToString() => Min == Max ? Min.ToString() : $"{Min}–{Max}";
    }
}
