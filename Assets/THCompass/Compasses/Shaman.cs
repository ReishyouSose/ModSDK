using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Shaman : Compass
    {
        public override BossID BossID => BossID.Shaman;

        public override AreaType Area => AreaType.Stone;

        public override bool BelongsToSlime => false;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            loot.AddUniqueRange(ObjectID.LegendarySwordGemstone);
        }
    }
}
