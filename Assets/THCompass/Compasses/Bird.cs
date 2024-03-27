using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Bird : Compass
    {
        public override BossID BossID => BossID.Bird;

        public override AreaType Area => AreaType.Nature;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.LargeShinyGlimmeringObject;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            ObjectID[] unique = new ObjectID[]
            {
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordGemstone,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
                ObjectID.CrystalCicada,
            };
            loot.AddUniqueRange(unique);
        }
    }
}
