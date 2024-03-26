using Assets.THCompass.DataStruct;

namespace Assets.THCompass.DropManager.Condition
{
    public abstract class DropCondition
    {
        public bool Reverse { get; private set; }

        protected abstract bool CheckMet(DropSource source);
        public DropCondition ReverseCondition()
        {
            Reverse = true;
            return this;
        }

        public bool IsMet(DropSource source) => Reverse ^ CheckMet(source);
    }
}
