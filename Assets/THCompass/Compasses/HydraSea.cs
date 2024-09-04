using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class HydraSea : Compass
    {
        public override BossID BossID => BossID.HydraSea;

        public override AreaType Area => AreaType.Sea;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.HydraBossSeaBait;

        public override ObjectID[] GetUniques()
        {
            return new ObjectID[]
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
