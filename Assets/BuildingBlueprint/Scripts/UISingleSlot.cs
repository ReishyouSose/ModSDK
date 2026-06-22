using Pug.UnityExtensions;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UISingleSlot : SlotUIBase
    {
        public ContainedObjectsBuffer Contained
        {
            get => contained;
            set
            {
                SetContainer(value);
                SetIcon(PugDatabase.GetObjectInfo(value.objectID, value.variation));
            }
        }
        private ContainedObjectsBuffer contained;

        protected override ContainedObjectsBuffer GetSlotObject() => contained;
        public void SetContainer(ContainedObjectsBuffer newOne) => contained = newOne;
        public void SetIcon(ObjectInfo info)
        {
            icon.sprite = info.icon;
            icon.transform.localPosition = info.iconOffset;
        }
    }
}
