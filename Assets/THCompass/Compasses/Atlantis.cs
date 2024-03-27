using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Atlantis : Compass
    {
        public override BossID BossID => BossID.Atlantis;
        public override AreaType Area => AreaType.Sea;

        public override bool BelongsToSlime => false;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            ObjectID[] unique = new ObjectID[]
            {
                ObjectID.ConchShellNecklace,
                ObjectID.SpineRing,
                ObjectID.OceanHeartNecklace,
                ObjectID.TurtleShell,
                ObjectID.TowerShellNecklace,
                ObjectID.CrystalCicada,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
            };
            loot.AddUniqueRange(unique);
        }
    }
}
