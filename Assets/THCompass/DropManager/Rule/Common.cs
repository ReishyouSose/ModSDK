using Assets.THCompass.DataStruct;
using System.Collections.Generic;

namespace Assets.THCompass.DropManager.Rule
{
    public class Common : DropRule
    {
        public readonly ObjectID itemID;
        public Common(ObjectID itemID) => this.itemID = itemID;
        protected override IEnumerable<DropInfo> DropSelf(DropSource source)
        {
            yield return new(itemID, GetDropCount(source.Rng));
        }
    }
}
