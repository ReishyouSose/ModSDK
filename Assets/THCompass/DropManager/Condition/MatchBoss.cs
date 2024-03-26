using Assets.THCompass.DataStruct;
using System.Linq;
using UnityEngine;

namespace Assets.THCompass.DropManager.Condition
{
    public class MatchBoss : DropCondition
    {
        public readonly BossID[] bossID;
        public MatchBoss(params BossID[] bossID) => this.bossID = bossID;
        protected override bool CheckMet(DropSource source)
        {
            bool contains = bossID.Contains(source.Compass.BossID);
            if (bossID.First() is BossID.Atlantis or BossID.Octopus or BossID.SlipperySlime)
            {
                Debug.Log(source.Compass.BossID + " " + contains);
            }
            return contains;
        }
    }
}
