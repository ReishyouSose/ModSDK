using Assets.THCompass.DataStruct;

namespace Assets.THCompass.DropManager.Condition
{
    public class IsTenTimes : DropCondition
    {
        protected override bool CheckMet(DropSource source) => source.Ten && source.Time == 0;
        public override string ToString() => IsReverse + "Ten";
    }
}
