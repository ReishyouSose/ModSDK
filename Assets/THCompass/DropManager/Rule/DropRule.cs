using Assets.THCompass.DataStruct;
using Assets.THCompass.DropManager.Condition;
using Assets.THCompass.Helper;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Assets.THCompass.DropManager.Rule
{
    public abstract class DropRule
    {
        public int minDrop = 1;
        public int maxDrop = 1;
        public float DropChance { get; private set; } = 1;
        public List<DropCondition> conditions = new();
        private readonly List<DropRule> SuccessChain = new();
        private readonly List<DropRule> FailureChain = new();
        public void SetDropChance(float x, int y) => DropChance = x / y;
        public void SetDropChance(float x) => DropChance = x;
        public DropRule WithCondition(params DropCondition[] condition)
        {
            conditions.AddRange(condition);
            return this;
        }

        public void OnSuccess(params DropRule[] drs) => SuccessChain.AddRange(drs);
        public void OnFailure(params DropRule[] drs) => FailureChain.AddRange(drs);
        public int GetDropCount(Random rng) => rng.NextInt(minDrop, maxDrop + 1);
        protected abstract IEnumerable<DropInfo> DropSelf(DropSource source);
        public List<DropInfo> Drop(DropSource source)
        {
            List<DropInfo> result = new();
            Drop(result, source);
            return result;
        }

        private void Drop(List<DropInfo> result, DropSource source)
        {
            if (IsMet(source))
            {
                if (source.Rng.NextBool(DropChance * (source.Ten ? 1.25f : 1)))
                {
                    result.AddRange(DropSelf(source));
                    foreach (DropRule dr in SuccessChain)
                    {
                        dr.Drop(result, source);
                    }
                }
                else
                {
                    foreach (DropRule dr in FailureChain)
                    {
                        dr.Drop(result, source);
                    }
                }
            }
        }
        public bool IsMet(DropSource source)
        {
            foreach (DropCondition cd in conditions)
            {
                if (!cd.IsMet(source)) return false;
            }
            return true;
        }
    }
}
