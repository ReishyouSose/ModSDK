using Assets.GeneralConfigMenu.RUIFramework;
using CoreLib.Data.Configuration;
using I2.Loc;
using System.Text;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigEntry : MonoBehaviour
    {
        [HideInInspector]
        public ConfigEntryBase ConfigEntry;

        public GameObject ServerChanger;
        public GameObject ClientChanger;
        public PugText ViewOnlyValue;
        public UIConfigAccessLevel UIConfigAccessLevel;
        private const string NeedReload = "GeneralConfigMenu/NeedReload";
        public string DefaultValue { get; protected set; }

        public RUIText Label { get; private set; }
        public void SetEntry(ConfigEntryBase configEntry)
        {
            gameObject.SetActive(true);
            ConfigEntry = configEntry;
            DefaultValue = ConfigEntry.DefaultValue.ToString();
            var scope = ConfigEntry.Scope;
            var level = scope.accessLevel;
            Label = GetComponent<RUIText>();
            Label.HoverText[0] += level;
            if (scope.requireReload)
            {
                Label.Hover = Color.red;
                Label.HoverText.Add(NeedReload);
                Label.SpecialHoverTextSnip += (index, text) =>
                {
                    if (index != 1)
                        return null;
                    if (text != NeedReload)
                        return null;
                    return new()
                    {
                        text = NeedReload,
                        formatFields = new string[0],
                        color = Color.red,
                    };
                };
            }
            Label.NeedHoverColor();
            Label.Hover = scope.requireReload ? Color.red : Color.white;
            UIConfigAccessLevel.SetLevel(level);
            var server = ServerChanger.GetComponent<RUIElement>();
            var client = ClientChanger.GetComponent<RUIElement>();
            switch (level)
            {
                case ConfigAccessLevel.ViewOnly:
                    ServerChanger.SetActive(false);
                    ClientChanger.SetActive(false);
                    ViewOnlyValue.Render(ConfigEntry.GetSerializedValue());
                    ViewOnlyValue.gameObject.SetActive(true);
                    break;
                case ConfigAccessLevel.Client:
                    ServerChanger.SetActive(false);
                    ViewOnlyValue.gameObject.SetActive(false);
                    client.HoverText.RemoveAt(1);
                    break;
                case ConfigAccessLevel.Server:
                case ConfigAccessLevel.Admin:
                    ViewOnlyValue.gameObject.SetActive(false);
                    server.AddEvent(RMouseEventType.RightDown, _ => TryServerToClient());
                    client.AddEvent(RMouseEventType.RightDown, _ => TryClientToServer());
                    break;
            }
            string key = ConfigEntry.Definition.Key;
            StringBuilder builder = new();
            builder.Append(ConfigEntry.ConfigFile.ConfigFilePath.Replace(".cfg", "/"))
                .Append(ConfigEntry.Definition.Section).Append('/').Append(key);
            var keyLocal = builder.ToString();
            if (LocalizationManager.TryGetTranslation(keyLocal, out _))
            {
                Label.Text.localize = true;
                Label.Text.Render(keyLocal);
            }
            else
                Label.Text.Render(key);
            builder.Append('/').Append("Description");
            keyLocal = builder.ToString();
            if (LocalizationManager.TryGetTranslation(keyLocal, out _))
            {
                Label.HoverText.Add(keyLocal);
            }
            else
            {
                builder = new();
                ConfigEntry.WriteDescription(builder);
                Label.HoverText.Add(builder.ToString());
            }
            SetChanger();
        }
        public virtual void SetChanger()
        {
        }
        public void TryServerToClient()
        {
            /*if (ServerEqualsClient())
                return;*/
            ServerToClient();
        }
        protected virtual void ServerToClient()
        {
        }
        public void TryClientToServer()
        {
            if (!ServerChanger.activeSelf || !CheckAdmin())
                return;
            /*if (ServerEqualsClient())
                return;*/
            ClientToServer();
        }
        protected virtual void ClientToServer()
        {
        }
        public virtual void ReceiveSync(string value)
        {
        }
        public virtual void ServerReset()
        {
        }
        public virtual void ClientReset()
        {
        }
        public bool AdminOnly => ConfigEntry.Scope.accessLevel == ConfigAccessLevel.Admin;
        public void SetClient(string value)
        {
            ConfigEntry.SetSerializedValue(value);
            ConfigEntry.ConfigFile.Save();
        }
        public static bool ClientSync => GeneralConfigMenuMod.config.ChangeClientWhenSync.Value;
        public static bool AutoStoC => GeneralConfigMenuMod.config.AutoStoC.Value;
        public static bool AutoCtoS => GeneralConfigMenuMod.config.AutoCtoS.Value;
        public void SetAndSendChange(string value)
        {
            var entry = ConfigEntry;
            entry.SetSerializedValue(value);
            GeneralConfigMenuMod.ConfigSync.SendConfigChange(entry);
        }

        public bool CheckAdmin() => !AdminOnly || Manager.main.player.adminPrivileges > 0;
        public virtual bool ValueEquals(string value) => ConfigEntry.GetSerializedValue() == value;
        /*public virtual bool ServerEqualsClient()
        {
            return false;
        }*/
    }
}
