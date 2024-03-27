using Assets.THCompass.DataStruct;

namespace Assets.THCompass.DropManager.Condition
{
    public class BlongsToSlime : DropCondition
    {
        protected override bool CheckMet(DropSource source) => source.Compass.BelongsToSlime;
        public override string ToString() => IsReverse + "BlongsToSlime";
    }
}
