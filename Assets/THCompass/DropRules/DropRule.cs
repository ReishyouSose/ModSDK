using Assets.THCompass.DropRules.DropConditions;
using Assets.THCompass.Helper;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Assets.THCompass.DropRules
{
    public struct DropInfo
    {
        public ObjectID itemID;
        public int stack;
        public DropInfo(ObjectID itemID, int stack)
        {
            this.itemID = itemID;
            this.stack = stack;
        }
    }
    public abstract class DropRule
    {
        public int minDrop = 1;
        public int maxDrop = 1;
        public float DropChance { get; private set; } = 1;
        public List<DropCondition> conditions = new();
        private readonly List<DropRule> SuccessChain = new();
        private readonly List<DropRule> FailureChain = new();
        public void SetDropChance(float x, int y) => DropChance = x / y;
        public void WithCondition(params DropCondition[] condition) => conditions.AddRange(condition);
        public void OnSuccess(params DropRule[] drs) => SuccessChain.AddRange(drs);
        public void OnFailure(params DropRule[] drs) => FailureChain.AddRange(drs);
        public int GetDropCount(Random rng) => rng.NextInt(minDrop, maxDrop + 1);
        protected abstract IEnumerable<DropInfo> DropSelf(Random rng);
        public List<DropInfo> Drop(Random rng)
        {
            List<DropInfo> result = new();
            Drop(result, rng);
            return result;
        }

        private void Drop(List<DropInfo> result, Random rng)
        {
            if ( rng.NextBool(DropChance))
            {
                result.AddRange(DropSelf(rng));
                foreach (DropRule dr in SuccessChain)
                {
                    dr.Drop(result, rng);
                }
            }
            else
            {
                foreach (DropRule dr in FailureChain)
                {
                    dr.Drop(result, rng);
                }
            }
        }
    }
}
