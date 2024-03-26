using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Devourer : Compass
    {
        public override BossID BossID => BossID.Devourer;

        public override AreaType Area => AreaType.Dirt;

        public override bool BelongsToSlime => false;
    }
}
