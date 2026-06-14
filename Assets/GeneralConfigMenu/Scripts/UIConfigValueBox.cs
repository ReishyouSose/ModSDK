using CoreLib.Data.Configuration;
using PugMod;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public abstract class UIConfigValueBox : MonoBehaviour
    {
        public SpriteRenderer SelectMarker;
        public GameObject ChangeMarker;

        [HideInInspector]
        public UIConfigEntry UEntry { get; private set; }

        [HideInInspector]
        public ConfigEntryBase Entry { get; private set; }

        [HideInInspector]
        public bool IsServerBox { get; private set; }

        [HideInInspector]
        public bool Editable
        {
            get => editable;
            set
            {
                editable = value;
                SelectMarker.color = value ? Color.white : Color.red;
            }
        }

        [HideInInspector]
        public string ValidValue
        {
            get => validValue;
            set
            {
                validValue = value;
                isDirty = true;
            }
        }

        protected bool editable;
        private string validValue;
        private string recordValue;
        private bool isDirty;

        public void BindEntry(UIConfigEntry entry, bool isServer)
        {
            UEntry = entry;
            Entry = entry.Entry;
            ValidValue = ConvertValue(Entry.GetSerializedValue());
            IsServerBox = isServer;
            Editable = !isServer;
            Init();
        }

        public virtual void Init() { }
        protected virtual string ConvertValue(string value)
        {
            return value;
        }

        /// <summary>
        /// 更新UI显示（子类实现）
        /// </summary>
        protected abstract void UpdateDisplayValue();

        public virtual void Update()
        {
            if (isDirty)
            {
                UpdateDisplayValue();
                isDirty = false;
            }
            ChangeMarker.SetActive(recordValue != validValue);
        }

        /// <summary>
        /// 用户修改值（通过UI输入框调用）
        /// </summary>
        public void ApplyUserChange(string value)
        {
            if (!editable)
                return;
            var newValue = TomlTypeConverter.ConvertToValue(value, Entry.SettingType);
            newValue = Entry.Description.AcceptableValues?.Clamp(newValue) ?? newValue;
            ValidValue = ConvertValue(newValue.ToString());
            var page = UEntry.OwnerPage;
            if (validValue != recordValue)
                page.AddChange(IsServerBox, UEntry, ValidValue.ToString());
            else
                page.RemoveChange(IsServerBox, UEntry);
        }

        /// <summary>
        /// 接收服务器广播（网络同步）
        /// </summary>
        public void OnReceiveSync(string value)
        {
            ValidValue = ConvertValue(value);
            SetRecord();
            UEntry.OwnerPage.RemoveChange(true, UEntry);
            ChangeMarker.SetActive(false);
        }
        public void SetRecord() => recordValue = validValue;
        public void ClearRecord()
        {
            if (validValue != recordValue)
                ValidValue = recordValue;
            recordValue = null;
        }
    }
}