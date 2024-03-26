using Assets.THCompass.DataStruct;
using System.Linq;

namespace Assets.THCompass.DropManager.Condition
{
    public class MatchBoss : DropCondition
    {
        public readonly BossID[] bossID;
        public MatchBoss(params BossID[] bossID) => this.bossID = bossID;
        protected override bool CheckMet(DropSource source) => bossID.Contains(source.Compass.BossID);
    }
}
