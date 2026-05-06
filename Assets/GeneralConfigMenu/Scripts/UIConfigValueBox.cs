using CoreLib.Data.Configuration;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public abstract class UIConfigValueBox : MonoBehaviour
    {
        [HideInInspector]
        public UIConfigEntry UEntry { get; private set; }

        [HideInInspector]
        public ConfigEntryBase Entry { get; private set; }

        [HideInInspector]
        public bool Editable;

        public void BindEntry(UIConfigEntry entry)
        {
            UEntry = entry;
            Entry = entry.Entry;
        }
        public void ReceiveValue(string value)
        {
            if (GeneralConfigMenuMod.config.ChangeClientWhenSync.Value)
            {
                Entry.SetSerializedValue(value);
                Entry.ConfigFile.Save();
                UpdateDisplayValue(value);
            }
        }
        public void ResetValue()
        {
            if (!Editable)
                return;
            SetValue(Entry.DefaultValue.ToString());
            UpdateDisplayValue(Entry.GetSerializedValue());
        }
        protected abstract void UpdateDisplayValue(string value);
        public virtual bool TryLocalizeServerValue(string value, out string key)
        {
            key = string.Empty;
            return false;
        }
        public void SetValue(string value)
        {
            Entry.SetSerializedValue(value);
            var player = Manager.main.player;
            bool shouldSync = Entry.Scope.ShouldSync;
            if (player == null || !shouldSync)
            {
                Entry.ConfigFile.Save();
                return;
            }
            if (player != null && shouldSync)
            {
                GeneralConfigMenuMod.ConfigSync.SendConfigChange(Entry);
                if (GeneralConfigMenuMod.config.AutoSave.Value)
                {
                    Entry.ConfigFile.Save();
                }
            }
        }
    }
}
