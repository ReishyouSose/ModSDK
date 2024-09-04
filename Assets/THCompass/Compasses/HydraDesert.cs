using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class HydraDesert : Compass
    {
        public override BossID BossID => BossID.HydraDesert;

        public override AreaType Area => AreaType.Desert;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.HydraBossDesertBait;

        public override ObjectID[] GetUniques()
        {
            return new ObjectID[]
            {
                ObjectID.BindingString,
                ObjectID.CavelingProphetMask,
                ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor,
                ObjectID.GodsentPantsArmor,
                ObjectID.FrozenFlame,
                ObjectID.WhiteWhistle,
                ObjectID.CrystalMeteorShardOffhand,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
                ObjectID.CrystalMeteorShard,
                ObjectID.AlienChest,
            };
        }

        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
        }
    }
}
