using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class SlipperySlime : Compass
    {
        public override BossID BossID => BossID.Slime;

        public override AreaType Area => AreaType.Sea;

        public override bool BelongsToSlime => true;
        public override void RegisterUniqueDrop(List<DropRule> common, List<DropRule> grand)
        {
            ObjectID[] unique = new ObjectID[]
            {
                 ObjectID.LegendaryBowPart1,
                 ObjectID.LegendaryBowPart2,
                 ObjectID.LegendaryBowPart3,
                 ObjectID.LegendaryBowParchment,
                 ObjectID.ConchShellNecklace,
                 ObjectID.SpineRing,
                 ObjectID.OceanHeartNecklace,
                 ObjectID.TurtleShell,
                 ObjectID.TowerShellNecklace,
            };
            common.AddRange(Drop.CommonMany(1, 1, 0.01f, unique));
            grand.Add(Drop.OneOf(1, 1, 1, unique));
        }
    }
}
