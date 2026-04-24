using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIConfigValueList : UIConfigValueBox
    {
        public PugText Text;
        public GameObject LeftActive;
        public GameObject LeftInactive;
        public GameObject RightActive;
        public GameObject RightInactive;
        private ButtonUIElement button;
        private string[] accepts;
        private int index;
        private int count;
        private string path;
        private void Awake()
        {
            button = GetComponent<ButtonUIElement>();
            var def = Entry.Definition;
            path = MiscHelper.GetLocalKey(Entry.ConfigFile.ConfigFilePath, def.Section, def.Key, "")[..^1];
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
            string key = $"{path}/{value}";
            Text.SetText(key, value);
            if (!visualOnly)
            {
                SetValue(value);
            }
        }

        public void SwitchIndex(int offset)
        {
            index = (index + count + offset) % count;
            CheckArrow();
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
            CheckArrow();
            SetState(true);
        }
        private void CheckArrow()
        {
            bool end = index == 0;
            LeftActive.SetActive(!end);
            LeftInactive.SetActive(end);
            end = index == count - 1;
            RightActive.SetActive(!end);
            RightInactive.SetActive(end);
        }
        protected override void ReceiveValue_Inner(string value)
        {
            FindIndex(value);
        }
    }
}
