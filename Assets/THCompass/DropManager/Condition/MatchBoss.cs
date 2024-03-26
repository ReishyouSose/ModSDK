using Assets.THCompass.DataStruct;
using System.Linq;

namespace Assets.THCompass.DropManager.Condition
{
    public class MatchBoss : DropCondition
    {
        public readonly BossID[] BossID;
        public MatchBoss(params BossID[] bossID) => BossID = bossID;
        protected override bool CheckMet(DropSource source) => BossID.Contains(source.Compass.BossID);
    }
}
