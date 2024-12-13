using System.Collections.Generic;
using I2.Loc;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class DescriptionInventorySlotUI : InventorySlotUI
    {
        public LocalizedString hoverDescWhenEmpty = (LocalizedString)"";


        public override List<TextAndFormatFields> GetHoverDescription()
        {
            ContainedObjectsBuffer slotObject = GetSlotObject();
            if (slotObject.objectID != ObjectID.None)
            {
                return base.GetHoverDescription();
            }
            
            if (hoverDescWhenEmpty.mTerm == "")
            {
                return base.GetHoverDescription(); 
            }

            return new List<TextAndFormatFields>
            {
                new TextAndFormatFields
                {
                    text = hoverDescWhenEmpty.mTerm
                }
            };
        }
    }
}