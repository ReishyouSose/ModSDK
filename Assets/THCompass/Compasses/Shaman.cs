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
        public override void RegisterUniqueDrop(List<DropRule> common, List<DropRule> grand)
        {
            common.Add(Drop.Common(ObjectID.LegendarySwordGemstone, 1, 1, 0.01f));
            grand.Add(Drop.Common(ObjectID.LegendarySwordGemstone));
        }
    }
}
