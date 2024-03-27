using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Assets.THCompass.Compasses
{
    public class Scarab : Compass
    {
        public override BossID BossID => BossID.Scarab;

        public override AreaType Area => AreaType.Desert;

        public override bool BelongsToSlime => false;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            ObjectID[] unique = new ObjectID[]
            {
                ObjectID.BindingString,
                ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor,
                ObjectID.GodsentPantsArmor,
                ObjectID.FrozenFlame,
                ObjectID.WhiteWhistle,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent
            };
            loot.AddRange(Drop.CommonMany(1, 1, 0.01f, unique).WithCondition(Drop.NotTenTime));
            loot.Add(Drop.Common(ObjectID.CrystalMeteorShard, 1, 8, 0.01f).WithCondition(Drop.NotTenTime));
            IEnumerable<DropRule> gr = Drop.CommonMany(1, 1, 1, unique);
            gr.AddItem(Drop.Common(ObjectID.CrystalMeteorShard, 1, 8));
            loot.Add(new OneOf(gr.ToArray()).WithCondition(Drop.IsTenTime));
        }
    }
}
