using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class PoisonSlime : Compass
    {
        public override BossID BossID => BossID.PoisonSlime;

        public override AreaType Area => AreaType.Nature;

        public override bool BelongsToSlime => true;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            ObjectID[] unique = new ObjectID[]
            {
                ObjectID.ConchShellNecklace,
                ObjectID.SpineRing,
                ObjectID.OceanHeartNecklace,
                ObjectID.TurtleShell,
                ObjectID.TowerShellNecklace,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
                ObjectID.LegendarySwordGemstone
            };
            loot.AddUniqueRange(unique);
        }
    }
}
