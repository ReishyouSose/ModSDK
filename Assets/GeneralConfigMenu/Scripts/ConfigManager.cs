using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using CoreLib.Data.Configuration;
using CoreLib.UserInterface;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ConfigManager : MonoBehaviour, IModUI
    {
        internal static ConfigManager Instance { get; private set; }
        public RUIScrollView ConfigPanel;
        public RUIText ConfigTemplate;
        public Transform EntryPanel;
        public PugText EntryLabel;
        public RUIScrollView EntryViewTemplate;
        public PugText SectionTemplate;
        public UIConfigEntryInput InputTemplate;
        public UIConfigEntryBool BoolTempalte;
        public UIConfigEntryList ListTemplate;
        public RUIButton Back;
        public RUIText ServerReset;
        public RUIText ClientReset;

        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        public bool Loaded { get; private set; }
        public bool PlayerExist { get; private set; }

        private Dictionary<ConfigFile, RUIScrollView> configViews;
        private List<MonoBehaviour> needChanges;

        private void Awake()
        {
            Instance = this;
            HideUI();
        }
        public void HideUI()
        {
            Root.SetActive(false);
            SetPlayerStateUI(true);
        }

        public void ShowUI()
        {
            Init();
            Main.ConfigSync?.JoinRequest();
            Root.SetActive(true);
            ConfigPanel.gameObject.SetActive(true);
            EntryPanel.gameObject.SetActive(false);
            SetPlayerStateUI(false);
            RenderLable();
        }

        private void SetPlayerStateUI(bool active)
        {
            if (!Loaded)
                return;
            foreach (var ui in needChanges)
            {
                ui.gameObject.SetActive(active);
            }
        }
        private void Init()
        {
            if (Loaded)
                return;
            Loaded = true;
            configViews = new();
            ConfigTemplate.gameObject.SetActive(false);
            EntryViewTemplate.gameObject.SetActive(false);
            ConfigPanel.Reload((panel, view) =>
            {
                foreach (var configFile in ConfigFile.AllConfigFilesReadOnly)
                {
                    if (configFile.ConfigFilePath.StartsWith("CoreLib"))
                        continue;
                    string localKey = configFile.ConfigFilePath.Replace(".cfg", string.Empty);
                    configFile.SaveOnConfigSet = false;
                    var newConfig = Instantiate(ConfigTemplate, view.transform);
                    newConfig.gameObject.SetActive(true);
                    newConfig.SetCustomData(configFile);
                    newConfig.GetComponent<OriginText>().Origin = localKey;
                    newConfig.NeedHoverColor();
                    newConfig.AddEvent(RMouseEventType.LeftDown, ConfigLeftDown);
                    panel.AddChild(newConfig.gameObject);

                    Dictionary<string, List<ConfigEntryBase>> sections = new();
                    foreach (var (def, entry) in configFile.Entries)
                    {
                        if (!sections.TryGetValue(def.Section, out var entries))
                            sections[def.Section] = entries = new();
                        entries.Add(entry);
                    }

                    var entryView = Instantiate(EntryViewTemplate, EntryPanel);
                    configViews.Add(configFile, entryView);
                    entryView.Reload((par, evt) =>
                    {
                        var temp = localKey;
                        foreach (var (section, entries) in sections)
                        {
                            var newSection = Instantiate(SectionTemplate, evt);
                            newSection.gameObject.SetActive(true);
                            string sectionLabel = section;
                            localKey = temp + "/" + sectionLabel;
                            if (LocalizationManager.TryGetTranslation(localKey, out _))
                            {
                                newSection.localize = true;
                                newSection.Render(localKey);
                            }
                            else
                                newSection.Render(sectionLabel);
                            par.AddChild(newSection.gameObject);

                            foreach (var entry in entries)
                            {
                                UIConfigEntry newEntry;
                                if (entry.SettingType == typeof(bool))
                                    newEntry = Instantiate(BoolTempalte, evt);
                                else
                                {
                                    if (TryMatchListType(entry, par, evt, entryView))
                                        continue;
                                    else
                                        newEntry = Instantiate(InputTemplate, evt);
                                }
                                newEntry.SetEntry(entry);
                                par.AddChild(newEntry.gameObject);
                            }
                        }
                    });
                    static void In3RowsInputer(Dictionary<RUIViewLocator, GameObject> children, int row,
                        out List<GameObject> server, out List<GameObject> client)
                    {
                        int down = row + 3;
                        server = new();
                        client = new();
                        foreach (var (local, child) in children)
                        {
                            int oriRow = local.OriRow;
                            if (oriRow <= row || oriRow > down)
                                continue;
                            if (child.TryGetComponent<UIConfigEntryInput>(out var input))
                            {
                                server.Add(input.ServerChanger);
                                client.Add(input.ClientChanger);
                            }
                        }
                    }
                    var children = entryView.children;
                    foreach (var (locator, child) in children)
                    {
                        if (child.TryGetComponent<UIConfigEntryList>(out var uc))
                        {
                            In3RowsInputer(children, locator.OriRow, out var server, out var client);
                            uc.ServerChanger.GetComponent<RUIExpand>().SetChangeWhenExpand(server.ToArray());
                            uc.ClientChanger.GetComponent<RUIExpand>().SetChangeWhenExpand(client.ToArray());
                        }
                    }
                }
            });
            Back.AddEvent(RMouseEventType.LeftDown, _ => BackLeftDown());
            ServerReset.NeedHoverColor();
            ClientReset.NeedHoverColor();
            ServerReset.AddEvent(RMouseEventType.LeftDown, ResetServer);
            ClientReset.AddEvent(RMouseEventType.LeftDown, ResetClient);

            var parent = Manager.ui.playerHealthBarUI.transform.parent;
            needChanges = new()
            {
                parent.GetComponentInChildren<PlayerHealthBarUI>(true),
                parent.GetComponentInChildren<MagicBarrierBarUI>(true),
                parent.GetComponentInChildren<PlayerHungerBarUI>(true),
                parent.GetComponentInChildren<PlayerManaBarUI>(true),
                parent.GetComponentInChildren<ConditionsContainerUI>(true),
                parent.GetComponentInChildren<MinionCountUI>(true),
                parent.GetComponentInChildren<InGameButtonHintsUI>(true)
            };
        }
        private void RenderLable()
        {
            foreach (Transform trans in ConfigPanel.View.transform)
            {
                var obj = trans.gameObject;
                if (obj.TryGetComponent<RUIText>(out var entry) && obj.TryGetComponent<OriginText>(out var origin))
                {
                    var ori = origin.Origin;
                    bool hasLocal = LocalizationManager.TryGetTranslation(ori, out var local);
                    entry.Text.Render(hasLocal ? local : ori, false, true);
                }
            }
        }
        private bool TryMatchListType(ConfigEntryBase entry, RUIScrollView par, Transform evt, RUIScrollView lockView)
        {
            string[] accepts = null;
            if (entry.SettingType.IsEnum)
            {
                var enums = Enum.GetValues(entry.SettingType);
                accepts = new string[enums.Length];
                int index = 0;
                foreach (var value in enums)
                {
                    accepts[index++] = value.ToString();
                }
            }
            else if (UIConfigEntryList.TryExtractAcceptableValues(entry, out string[] values))
            {
                accepts = values;
            }
            if (accepts == null)
            {
                return false;
            }
            var newEntry = Instantiate(ListTemplate, evt);
            newEntry.SetEntry(entry);
            newEntry.LoadExpand(accepts, lockView);
            par.AddChild(newEntry.gameObject);
            return true;
        }
        public void BackLeftDown()
        {
            ConfigPanel.gameObject.SetActive(true);
            EntryPanel.gameObject.SetActive(false);
        }
        private void ConfigLeftDown(GameObject go)
        {
            ConfigPanel.gameObject.SetActive(false);
            EntryPanel.gameObject.SetActive(true);
            var text = go.GetComponent<RUIText>();
            var key = text.customData[0] as ConfigFile;
            EntryLabel.Render(text.Text.displayedTextString);
            foreach (var (configFile, view) in configViews)
            {
                view.gameObject.SetActive(configFile == key);
            }
        }
        private void ResetServer(GameObject go)
        {
            bool auto = Main.config.AutoStoC.Value;
            var children = configViews.First(x => x.Value.gameObject.activeInHierarchy).Value.children;
            foreach (var (_, child) in children)
            {
                if (child.TryGetComponent<UIConfigEntry>(out var ue))
                {
                    if (!ue.ConfigEntry.Scope.ShouldSync)
                        continue;
                    if (ue.ValueEquals(ue.DefaultValue))
                        continue;
                    ue.ServerReset();
                    if (auto)
                        ue.TryServerToClient();
                }
            }
        }
        private void ResetClient(GameObject go)
        {
            bool auto = Main.config.AutoCtoS.Value;
            var children = configViews.First(x => x.Value.gameObject.activeInHierarchy).Value.children;
            foreach (var (_, child) in children)
            {
                if (child.TryGetComponent<UIConfigEntry>(out var ue))
                {
                    if (ue.ConfigEntry.Scope.AccessLevel == ConfigAccessLevel.ViewOnly)
                        continue;
                    if (ue.ValueEquals(ue.DefaultValue))
                        continue;
                    ue.ClientReset();
                    if (auto)
                        ue.TryClientToServer();
                }
            }
        }
        public static bool TryConvertConfig(ConfigEntryBase config, out string data)
        {
            StringBuilder builder = new();
            builder.Append(config.ConfigFile.ConfigFilePath.Replace(".cfg", string.Empty)).Append('|');
            var def = config.Definition;
            builder.Append(def.Section).Append('|').Append(def.Key).Append('|').Append(config.GetSerializedValue());
            data = builder.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            if (byteArray.Length > FixedString128Bytes.UTF8MaxLengthInBytes)
            {
                Debug.LogError(data + "\nToo long config bytes!");
                return false;
            }
            return true;
        }
        internal void TryReceiveSync(int playerIndex, string data)
        {
            if (playerIndex == Manager.main.player.playerIndex)
                return;
            string[] info = data.Split('|');
            if (info.Length != 4)
            {
                Debug.Log("Try spilt " + data + " Failed");
                return;
            }
            string config = info[0], section = info[1], key = info[2], value = info[3];
            foreach (var (configFile, view) in configViews)
            {
                if (configFile.ConfigFilePath.StartsWith(config))
                {
                    foreach (var (_, go) in view.children)
                    {
                        if (go.TryGetComponent<UIConfigEntry>(out var uc))
                        {
                            var def = uc.ConfigEntry.Definition;
                            if (def.Section == section && def.Key == key)
                            {
                                uc.ReceiveSync(value);
                                if (UIConfigEntry.ClientSync)
                                {
                                    uc.TryServerToClient();
                                }
                                Debug.Log("Receive " + data);
                                return;
                            }
                        }
                    }
                }
            }
            Debug.Log(data + " can't find config entry in UI");
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (EntryPanel.gameObject.activeInHierarchy)
                {
                    EntryPanel.gameObject.SetActive(false);
                    ConfigPanel.gameObject.SetActive(true);
                    return;
                }
                HideUI();
            }
        }
    }
}
