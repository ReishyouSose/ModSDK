using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class WallSlime : Compass
    {
        public override BossID BossID => BossID.WallSlime;

        public override AreaType Area => AreaType.Passage;

        public override bool BelongsToSlime => false;
        public override ObjectID BossSummoner => ObjectID.WallBossSummoningItem;

        public override ObjectID[] GetUniques()
        {
            return new ObjectID[]
            {
                ObjectID.PassageCraftingStatue,
                ObjectID.PassageChest
            };
        }

        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
        }
    }
}
