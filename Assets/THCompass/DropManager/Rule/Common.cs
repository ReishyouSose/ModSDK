using Assets.THCompass.DataStruct;
using CoreLib.Drops;
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
        public override IEnumerable<DropTableInfo> SelfToInfo()
        {
            yield return new DropTableInfo(itemID, minDrop, maxDrop, 1f, true);
        }
        public override string ToString() => "CommonDrop" + itemID;
    }
}
