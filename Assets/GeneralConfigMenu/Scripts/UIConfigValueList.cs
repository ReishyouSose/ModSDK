using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueList : UIConfigValueBox
    {
        public PugText Text;
        private string[] accepts;
        private int index;
        private int count;
        private string localizationPath;

        public override void Init()
        {
            name = "List" + (IsServerBox ? "(Server)" : "(Client)");
            var def = Entry.Definition;
            localizationPath = MiscHelper.GetLocalKey(Entry.ConfigFile.ConfigFilePath, def.Section, def.Key, "");
            GetComponent<ButtonUIElement>().optionalTitle.mTerm = "GeneralConfigMenu/" + (IsServerBox ? "ServerValue" : "ClientValue");
        }

        public void SetAccepts(string[] values)
        {
            accepts = values;
            count = values.Length;
        }

        public void SwitchIndex(int offset)
        {
            if (!editable)
            {
                UEntry.ShowUnEditableWarning();
                return;
            }
            index = (index + count + offset) % count;
            string value = accepts[index];
            ApplyUserChange(value);
        }

        private void FindIndex(string value)
        {
            for (int i = 0; i < count; i++)
            {
                if (accepts[i] == value)
                {
                    index = i;
                    break;
                }
            }
            Text.SetText(localizationPath + value, value);
        }

        protected override void UpdateDisplayValue()
        {
            FindIndex(ValidValue.ToString());
        }
    }
}