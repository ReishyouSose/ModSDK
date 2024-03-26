using System;

namespace Assets.THCompass.Helper
{
    public static class RandomHelper
    {
        public static Random Rng => new((int)PugRandom.GetSeed());
        public static bool GetRngBool(this float x)
        {
            if (x <= 0) return false;
            if (x >= 1) return true;
            int y = 1;
            while (x != (int)x)
            {
                x *= 10;
                y *= 10;
            }
            return Rng.Next(0, y) < x;
        }
        /// <summary>
        /// 左闭右闭
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRngInt(int min, int max) => Rng.Next(min, max + 1);

        private static int LinearDecrease(Random rng, int min, int max)
        {
            int diff = max - min;
            double x = rng.NextDouble(); //产生一个0-1之间的随机数
            // Math.Sqrt是为了对1做逆运算，使得结果为线性递减分布
            return (int)(diff + 1 - diff * Math.Sqrt(1 - x)) + min;
        }
        /// <summary>
        /// 正态分布
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int NormalDistribution(Random random, int min, int max)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (int)Math.Round(min + randStdNormal * (max - min) / 2);
        }
    }
}
