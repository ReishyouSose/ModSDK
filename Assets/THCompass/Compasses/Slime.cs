using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Rule;
using Assets.THCompass.Helper;
using System.Collections.Generic;

namespace Assets.THCompass.Compasses
{
    public class Slime : Compass
    {
        public override BossID BossID => BossID.Slime;

        public override AreaType Area => AreaType.Dirt;

        public override bool BelongsToSlime => true;
        public override void RegisterUniqueDrop(List<DropRule> loot)
        {
            ObjectID[] dirt = new ObjectID[3]
            {
                ObjectID.ParsecPalsDolls,
                ObjectID.ColossCicada,
                ObjectID.AmmoniteNecklace
            };
            loot.AddUniqueRange(dirt);
        }
    }
}
