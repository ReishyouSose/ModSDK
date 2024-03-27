using Assets.THCompass.DataStruct;
using System.Collections.Generic;
using System.Linq;

namespace Assets.THCompass.DropManager.Rule
{
    public class OneOf : DropRule
    {
        private readonly DropRule[] rules;
        public OneOf(params ObjectID[] items)
        {
            List<DropRule> rs = new();
            foreach (ObjectID item in items)
            {
                rs.Add(new Common(item)
                {
                    minDrop = minDrop,
                    maxDrop = maxDrop
                });
            }
            rules = rs.ToArray();
        }
        public OneOf(params DropRule[] rules) => this.rules = rules;
        protected override IEnumerable<DropInfo> DropSelf(DropSource source)
        {
            foreach (DropInfo info in rules[source.Rng.NextInt(rules.Count())].Drop(source))
            {
                yield return info;
            }
        }
        public override string ToString() => $"OneOf {rules.Count()}";
    }
}
