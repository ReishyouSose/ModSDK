using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Hive : Compass
    {
        public override BossID BossID => BossID.Hive;

        public override AreaType Area => AreaType.Dirt;

        public override bool BelongsToSlime => false;
    }
}
