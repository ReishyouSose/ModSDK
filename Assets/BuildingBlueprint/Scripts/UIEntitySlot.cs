using Pug.UnityExtensions;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UIEntitySlot : SlotUIBase
    {
        public ContainedObjectsBuffer Contained
        {
            get => contained;
            set
            {
                contained = value;
                var info = PugDatabase.GetObjectInfo(value.objectID, value.variation);
                icon.sprite = info.icon;
                icon.transform.localPosition = info.iconOffset;
            }
        }
        private ContainedObjectsBuffer contained;

        protected override ContainedObjectsBuffer GetSlotObject() => contained;
    }
}
