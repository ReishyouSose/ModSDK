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
        public override void RegisterUniqueDrop(List<DropRule> common, List<DropRule> grand)
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
                ObjectID.LegendarySwordGemstone
            };
            common.AddRange(Drop.CommonMany(1, 1, 0.01f, unique));
            grand.Add(Drop.OneOf(1, 1, 1, unique));
        }
    }
}
