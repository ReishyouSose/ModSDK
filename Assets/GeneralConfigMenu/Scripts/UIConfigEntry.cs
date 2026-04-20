using Assets.GeneralConfigMenu.RUIFramework;
using CoreLib.Data.Configuration;
using I2.Loc;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(RUIText))]
    public class UIConfigEntry : MonoBehaviour
    {
        [HideInInspector]
        public ConfigEntryBase ConfigEntry;

        public GameObject ServerChanger;
        public GameObject ClientChanger;
        public PugText ViewOnlyValue;
        public UIConfigAccessLevel UIConfigAccessLevel;
        private const string NeedReload = "GeneralConfigMenu/NeedReload";
        private const string HasExtraConfig = "GeneralConfigMenu/HasExtraConfig";
        private const string AccessLevelKey = "GeneralConfigMenu_AccessLevel/";
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
            var hoverText = Label.HoverText;
            hoverText.Insert(0, AccessLevelKey + level);
            hoverText.Insert(1, AccessLevelKey + level + "Desc");
            Label.SpecialHoverTextSnip += (index, text) =>
            {
                if (index >= 2)
                    return null;
                return new()
                {
                    text = text,
                    formatFields = new string[0],
                    color = Color.yellow,
                };
            };
            if (scope.requireReload)
            {
                Label.Hover = Color.red;
                hoverText.Add(NeedReload);
                Label.SpecialHoverTextSnip += (index, text) =>
                {
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
            var def = configEntry.Definition;
            string key = def.Key;
            var tags = ConfigEntry.Description.Tags;
            var keyLocal = tags.FirstOrDefault(x => x is LocalizationOverride) is LocalizationOverride lfx ? lfx.Key :
                MiscHelper.GetLocalKey(ConfigEntry.ConfigFile.ConfigFilePath, def.Section, key);
            Label.Text.SetText(keyLocal, key);
            bool hasLocalize = LocalizationManager.TryGetTranslation(keyLocal, out _);
            keyLocal += "Desc";
            if (LocalizationManager.TryGetTranslation(keyLocal, out _))
            {
                hoverText.Add(keyLocal);
            }
            else
            {
                StringBuilder builder = new();
                ConfigEntry.WriteDescription(builder);
                hoverText.Add(builder.ToString());
            }
            if (tags.Contains(ConfigData.HasExtraConfig))
            {
                hoverText.Add(HasExtraConfig);
                Label.SpecialHoverTextSnip += (index, text) =>
                {
                    if (text != HasExtraConfig)
                        return null;
                    return new()
                    {
                        text = HasExtraConfig,
                        formatFields = new string[0],
                        color = Color.cyan,
                    };
                };
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
