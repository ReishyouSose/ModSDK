using Assets.THCompass.Compasses;
using Unity.Mathematics;

namespace Assets.THCompass.DataStruct
{
    public readonly struct DropSource
    {
        public readonly Random Rng;
        public readonly Compass Compass;
        public readonly bool Ten;
        public DropSource(Compass compass, bool ten, Random rng)
        {
            Compass = compass;
            Ten = ten;
            Rng = rng;
        }
    }
}
