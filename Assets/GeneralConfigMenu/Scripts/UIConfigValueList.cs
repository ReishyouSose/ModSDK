using I2.Loc;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueList : UIConfigValueBox
    {
        public PugText Text;
        private ButtonUIElement button;
        private string[] accepts;
        private int index;
        private int count;
        private string path;
        private void Awake()
        {
            button = GetComponent<ButtonUIElement>();
            FindIndex(Entry.GetSerializedValue());
        }
        private void Update()
        {
            button.canBeClicked = Editable;
        }
        public void SetAccepts(string[] accepts)
        {
            this.accepts = accepts;
            count = accepts.Length;
        }

        private void SetState(bool visualOnly)
        {
            var value = accepts[index];
            string key = path + value;
            Text.SetText(key, value);
            if (!visualOnly)
            {
                SetValue(value);
            }
        }

        public void SwitchIndex(int offset)
        {
            index = (index + count + offset) % count;
            SetState(false);
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
            SetState(true);
        }
        protected override void UpdateDisplayValue(string value)
        {
            FindIndex(value);
        }
        public override bool TryLocalizeServerValue(string value, out string key)
        {
            key = path + value;
            bool result = LocalizationManager.TryGetTranslation(key, out _);
            Debug.Log((key, result));
            return result;
        }
        public override void Init()
        {
            var def = Entry.Definition;
            path = MiscHelper.GetLocalKey(Entry.ConfigFile.ConfigFilePath, def.Section, def.Key, "");
        }
    }
}