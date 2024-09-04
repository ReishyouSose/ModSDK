using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class HydraNature : Compass
    {
        public override BossID BossID => BossID.HydraNature;

        public override AreaType Area => AreaType.Nature;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.HydraBossNatureBait;

        public override ObjectID[] GetUniques()
        {
            return new ObjectID[]
            {
                ObjectID.LegendarySwordGemstone,
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
                ObjectID.CrystalCicada,
                ObjectID.AlienChest,
            };
        }

        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
        }
    }
}
