using Assets.THCompass.DataStruct;

namespace Assets.THCompass.Compasses
{
    public class Slime : Compass
    {
        public override BossID BossID => BossID.Slime;

        public override AreaType Area => AreaType.Dirt;

        public override bool BelongsToSlime => true;
    }
}
