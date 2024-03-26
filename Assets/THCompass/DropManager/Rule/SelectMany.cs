using Assets.THCompass.DataStruct;
using System;
using System.Collections.Generic;

namespace Assets.THCompass.DropManager.Rule
{
    public class SelectMany : DropRule
    {
        public int selectMax;
        public int selectMin;
        public readonly ObjectID[] items;
        public SelectMany(params ObjectID[] items) => this.items = items;

        protected override IEnumerable<DropInfo> DropSelf(DropSource source)
        {
            throw new NotImplementedException();
        }
    }
}
