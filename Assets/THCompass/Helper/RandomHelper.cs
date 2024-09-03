using System;
using Random =  Unity.Mathematics.Random;

namespace Assets.THCompass.Helper
{
    public static class RandomHelper
    {
        public static Random GetRng() => PugRandom.GetRng();
        /// <returns>(1/x)%</returns>
        public static bool NextBool(this Random rng, int x) => rng.NextBool(1, x);
        /// <returns>(x/y)%</returns>
        public static bool NextBool(this Random rng, int x, int y) => rng.NextInt(y) < x;
        public static bool NextBool(this Random rng, float x) => rng.NextDouble() < x;
        public static bool NextBool(this Random rng, double x) => rng.NextDouble() < x;

        /// <summary>线性递减</summary>
        public static int LinearDecrease(Random rng, int min, int max)
        {
            int diff = max - min;
            double x = rng.NextDouble(); //产生一个0-1之间的随机数
            // Math.Sqrt是为了对1做逆运算，使得结果为线性递减分布
            return (int)(diff + 1 - diff * Math.Sqrt(1 - x)) + min;
        }
        /// <summary>
        /// 正态分布
        /// </summary>
        public static int NormalDistribution(Random random, int min, int max)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (int)Math.Round(min + randStdNormal * (max - min) / 2);
        }
    }
}
