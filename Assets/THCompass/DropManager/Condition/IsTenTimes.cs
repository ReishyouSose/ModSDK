using Assets.THCompass.DataStruct;
using System;

namespace Assets.THCompass.DropManager.Condition
{
    public class IsTenTimes : DropCondition
    {
        protected override bool CheckMet(DropSource source) => source.Ten;
    }
}
