using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class CoreCommander : Compass
    {
        public override BossID BossID => BossID.CoreCommander;

        public override AreaType Area => AreaType.Shimmer;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.CoreBossSummoningItem;

        public override ObjectID[] GetUniques()
        {
            return new ObjectID[]
            {
                ObjectID.ParsecPalsDolls,
                ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace,
                ObjectID.LegendarySwordGemstone,
                ObjectID.CavelingDoll,
                ObjectID.LegendarySwordBlade,
                ObjectID.CavelingMothersRing,
                ObjectID.AncientGuardianNecklace,
                ObjectID.OldSporeMask,
                ObjectID.MoldCicadaWithoutSickle,
                ObjectID.LegendaryBowPart1,
                ObjectID.LegendaryBowPart2,
                ObjectID.LegendaryBowPart3,
                ObjectID.LegendaryBowParchment,
                ObjectID.ConchShellNecklace,
                ObjectID.SpineRing,
                ObjectID.OceanHeartNecklace,
                ObjectID.TurtleShell,
                ObjectID.TowerShellNecklace,
                ObjectID.BindingString,
                ObjectID.CavelingProphetMask,
                ObjectID.GodsentHelm,
                ObjectID.GodsentBreastArmor,
                ObjectID.GodsentPantsArmor,
                ObjectID.FrozenFlame,
                ObjectID.WhiteWhistle,
                ObjectID.CrystalCicada,
                ObjectID.AgarthaReport,
                ObjectID.CrystalTent,
                ObjectID.PassageCraftingStatue,
                ObjectID.PassageChest
            };
        }

        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
        }
    }
}
