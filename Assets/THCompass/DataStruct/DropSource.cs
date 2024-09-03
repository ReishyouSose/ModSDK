using Assets.THCompass.Compasses;
using Unity.Mathematics;

namespace Assets.THCompass.DataStruct
{
    public readonly struct DropSource
    {
        public readonly Random Rng => PugRandom.GetRng();
        public readonly Compass Compass;
        public readonly bool Ten;
        public readonly int Time;
        public DropSource(Compass compass, bool ten,int time)
        {
            Compass = compass;
            Ten = ten;
            Time = time;
        }
    }
}
