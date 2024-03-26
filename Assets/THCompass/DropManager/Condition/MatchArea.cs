using Assets.THCompass.DataStruct;
using System;
using System.Linq;

namespace Assets.THCompass.DropManager.Condition
{
    public class MatchArea : DropCondition
    {
        public readonly AreaType[] Area;
        public MatchArea(params AreaType[] area) => Area = area;
        protected override bool CheckMet(DropSource source) => Area.Contains(source.Compass.Area);
    }
}
