using Assets.THCompass.DataStruct;
using System.Collections.Generic;
using System.Linq;

namespace Assets.THCompass.DropManager.Rule
{
    public class SelectMany : DropRule
    {
        public int selectMax;
        public int selectMin;
        public readonly DropRule[] rules;
        public SelectMany(params DropRule[] rules) => this.rules = rules;

        protected override IEnumerable<DropInfo> DropSelf(DropSource source)
        {
            int select = source.Rng.NextInt(selectMin, selectMax + 1);
            int count = rules.Count();
            HashSet<int> selector = new();
            while (selector.Count < select)
            {
                selector.Add(source.Rng.NextInt(count));
            }
            foreach (int index in selector)
            {
                foreach (DropInfo info in rules[index].Drop(source))
                {
                    yield return info;
                }
            }
        }
        public override string ToString() => $"Select {selectMin}~{selectMax} in {rules.Count()}";
    }
}
