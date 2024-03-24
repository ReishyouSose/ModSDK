using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Assets.THCompass.DropRules
{
    public class OneOf : DropRule
    {
        public readonly ObjectID[] items;
        public OneOf(int min, int max, params ObjectID[] items)
        {
            minDrop = min;
            maxDrop = max;
            this.items = items;
        }
        protected override IEnumerable<DropInfo> DropSelf(Random rng)
        {
            ObjectID itemID = items[rng.NextInt(items.Count())];
            yield return new(itemID, GetDropCount(rng));
        }
    }
}
