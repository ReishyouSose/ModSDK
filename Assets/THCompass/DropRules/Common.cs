using System.Collections.Generic;

namespace Assets.THCompass.DropRules
{
    public class Common : DropRule
    {
        public readonly ObjectID itemID;
        protected override IEnumerable<DropInfo> DropSelf(Unity.Mathematics.Random rng)
        {
            yield return new(itemID, GetDropCount(rng));
        }
    }
}
