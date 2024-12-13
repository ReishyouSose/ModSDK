using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using Assets.GeneralConfigMenu.UIByLimoka;
using CoreLib.Data.Configuration;
using CoreLib.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.GeneralConfigMenu.MonoBehaivours
{
    public class ModConfigMenu : UIelement, IModUI
    {
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        public GameObject ModMenu;
        public GameObject ConfigMenu;
        public RUIPageSwitcher ModView;
        public Transform ModTemplate;
        public RUIPageSwitcher FileView;
        public Transform FileTemplate;
        public RUIPageSwitcher ConfigView;
        public Transform ConfigTemplate;
        public RUIButton Back;
        public GameObject DescPanel;
        public PugText Description;
        public GameObject EnumPage;
        public UIRangeTip RangeTip;
        public ConfigChangerPage Changer;

        private string currentMod, currentFile;
        //mod, file, section
        private Dictionary<string, Dictionary<string, Dictionary<string, List<ConfigEntryBase>>>> configData;

        internal static ModConfigMenu Ins { get; private set; }

        private void Awake()
        {
            HideUI();
            ModMenu.SetActive(true);
            ConfigMenu.SetActive(false);
        }
        private void Start()
        {
            Ins = this;
            configData = new();
            Transform modViewTrans = ModView.transform;
            ModView.ReLoadPage(() =>
            {
                foreach (var cfile in ConfigFile.AllConfigFilesReadOnly)
                {
                    string path = cfile.ConfigFilePath;
                    int index = path.IndexOf('/');
                    string mod = path[..index];
                    string file = path[(index + 1)..].Replace(".cfg", string.Empty);
                    if (!configData.TryGetValue(mod, out var files))
                    {
                        configData[mod] = files = new();
                        var modEntry = Instantiate(ModTemplate, modViewTrans);
                        modEntry.GetComponent<PugText>().textString = mod;
                        var button = modEntry.GetComponent<RUIButton>();
                        button.AddEvent(RMouseEventType.LeftDown, LoadFileView);
                        ModView.AddElement(modEntry);
                    }
                    if (!files.TryGetValue(file, out var sections))
                        files[file] = sections = new();
                    foreach (var (def, entry) in cfile)
                    {
                        string section = def.Section;
                        if (!sections.TryGetValue(section, out var configs))
                            sections[section] = configs = new();
                        configs.Add(entry);
                    }
                }
            });
            Back.AddEvent(RMouseEventType.LeftDown, BackToModMenu);
        }

        private void BackToModMenu(GameObject go)
        {
            foreach (Transform child in Changer.transform)
            {
                if (child.gameObject.activeInHierarchy)
                    Destroy(child.gameObject);
            }
            ModMenu.SetActive(true);
            ConfigMenu.SetActive(false);
            Description.Render(string.Empty);
            Changer.gameObject.SetActive(false);
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
            if (ConfigMenu.activeInHierarchy)
            {
                BackToModMenu(ConfigMenu);
            }
        }
        public void LoadFileView(GameObject go)
        {
            Transform fileViewTrans = FileView.transform;
            var key = go.GetComponent<PugText>().textString;
            currentMod = key;
            FileView.ReLoadPage(() =>
            {
                foreach (var (file, _) in configData[key])
                {
                    var fileEntry = Instantiate(FileTemplate, fileViewTrans);
                    fileEntry.GetComponent<PugText>().textString = file;
                    fileEntry.gameObject.SetActive(true);
                    fileEntry.GetComponent<RUIButton>().AddEvent(RMouseEventType.LeftDown, LoadConfigView);
                    FileView.AddElement(fileEntry);
                }
            });
        }
        private void LoadConfigView(GameObject go)
        {
            var key = go.GetComponent<PugText>().textString;
            currentFile = key;
            Transform configViewTrans = ConfigView.transform;
            ConfigView.ReLoadPage(() =>
            {
                foreach (var (section, entries) in configData[currentMod][currentFile])
                {
                    foreach (var entry in entries)
                    {
                        Transform child = Instantiate(ConfigTemplate, configViewTrans);
                        UIConfigEntry config = child.GetComponent<UIConfigEntry>();
                        config.Label.Render(entry.Definition.Key);
                        config.Value.Render(entry.GetSerializedValue());
                        config.ConfigEntry = entry;
                        RUIButton button = child.GetComponent<RUIButton>();
                        for (int i = 0; i < 10; i++)
                        {
                            button.HoverText += entry.Definition.Key;
                        }
                        button.AddEvent(RMouseEventType.LeftDown, LoadDescription);
                        button.AddEvent(RMouseEventType.LeftDown, LoadChanger);
                        ConfigView.AddElement(child);
                    }
                }
            });
            ModMenu.SetActive(false);
            ConfigMenu.SetActive(true);
        }
        private void LoadDescription(GameObject go)
        {
            StringBuilder builder = new();
            var config = go.GetComponent<UIConfigEntry>().ConfigEntry;
            if (config.IsNeedReload())
            {
                builder.WirteNeedReloadText();
            }
            config.WriteDescription(builder);
            DescPanel.SetActive(true);
            EnumPage.SetActive(false);
            Description.Render(builder.ToString());
        }
        private void LoadChanger(GameObject go)
        {
            foreach (Transform child in Changer.transform)
            {
                if (child.gameObject.activeInHierarchy)
                    Destroy(child.gameObject);
            }
            Changer.gameObject.SetActive(true);
            var uc = go.GetComponent<UIConfigEntry>();
            var config = uc.ConfigEntry;
            Type type = config.SettingType;
            var changerTrans = Changer.transform;
            RangeTip.gameObject.SetActive(false);
            GameObject changer;
            if (type == typeof(bool))
            {
                var entry = config as ConfigEntry<bool>;
                var boolean = Instantiate(Changer.Boolean, changerTrans);
                changer = boolean.gameObject;
                boolean.ToggleAtFirst = entry.Value;
                boolean.AddEvent(RMouseEventType.LeftDown, BooleanClick);
            }
            else if (type.IsEnum)
            {
                var enumable = Instantiate(Changer.Enumable, changerTrans);
                changer = enumable.gameObject;
                enumable.GetComponentInChildren<PugText>().Render(config.GetSerializedValue());
                enumable.AddEvent(RMouseEventType.LeftDown, EnumableClick);
            }
            else
            {
                RangeTip.gameObject.SetActive(true);
                var input = Instantiate(Changer.Input, changerTrans);
                changer = input.gameObject;
                RangeTip.TryRange(config.Description.AcceptableValues);
                BetterInputField inputField = input.GetComponent<BetterInputField>();
                inputField.SetInputText(config.GetSerializedValue());
                inputField.onTextChanged += InputSubmit;
            }
            changer.GetComponent<UIConfigChanger>().UIConfigEntry = uc;
            changer.SetActive(true);
        }
        private void BooleanClick(GameObject go)
        {
            UIConfigEntry uc = go.GetComponent<UIConfigChanger>().UIConfigEntry;
            var config = uc.ConfigEntry as ConfigEntry<bool>;
            bool active = go.GetComponent<RUIButton>().IsToggle;
            config.Value = active;
            uc.Value.Render(active.ToString().ToLower());
            SendConfig(config);
        }

        private void EnumableClick(GameObject go)
        {
            bool active = go.GetComponent<RUIButton>().IsToggle;
            if (active)
            {
                UIConfigEntry uc = go.GetComponent<UIConfigChanger>().UIConfigEntry;
                var config = uc.ConfigEntry;
                RUIPageSwitcher enumPage = EnumPage.GetComponentInChildren<RUIPageSwitcher>();
                var pageTrans = enumPage.transform;
                enumPage.ReLoadPage(() =>
                {
                    Array accepts = Enum.GetValues(config.SettingType);
                    bool find = false;
                    foreach (var v in accepts)
                    {
                        var selectable = Instantiate(enumPage.Template, pageTrans);
                        var originText = selectable.GetComponentInChildren<PugText>();
                        string value = v.ToString();
                        originText.Render(value);
                        var uconf = selectable.gameObject.AddComponent<UIConfigChanger>();
                        uconf.UIConfigEntry = uc;
                        uconf.Origin = go;
                        if (!find && value == config.GetSerializedValue())
                        {
                            selectable.GetComponent<RUIButton>().SetState(find = !find);
                        }
                        selectable.GetComponent<RUIButton>().AddEvent(RMouseEventType.LeftDown, EnumEntryClick);
                        enumPage.AddElement(selectable);
                    }
                });
                EnumPage.SetActive(true);
                DescPanel.SetActive(false);
            }
            else
            {
                EnumPage.SetActive(false);
                DescPanel.SetActive(true);
            }
        }
        private void EnumEntryClick(GameObject go)
        {
            var uChanger = go.GetComponent<UIConfigChanger>();
            UIConfigEntry uc = uChanger.UIConfigEntry;
            var config = uc.ConfigEntry;
            config.SetSerializedValue(go.GetComponentInChildren<PugText>().textString);
            string value = config.GetSerializedValue();
            var origin = uChanger.Origin;
            origin.GetComponentInChildren<PugText>().Render(value);
            origin.GetComponent<RUIButton>().SetState(false);
            uc.Value.Render(value);
            DescPanel.SetActive(true);
            EnumPage.SetActive(false);
            SendConfig(config);
        }
        private void InputSubmit(GameObject go, string value)
        {
            UIConfigEntry uc = go.GetComponent<UIConfigChanger>().UIConfigEntry;
            var config = uc.ConfigEntry;
            config.SetSerializedValue(value);
            var str = config.GetSerializedValue();
            uc.Value.Render(str);
            go.GetComponent<BetterInputField>().SetInputText(str);
            SendConfig(config);
        }
        internal void TryRecieveSync(int playerIndex, string mod, string file, string section, string key, string value)
        {
            Debug.Log($"{mod}/{file}/{section}/{key} to {value}");
            if (playerIndex == Manager.main.player.playerIndex)
                return;
            if (!configData.TryGetValue(mod, out var files))
                return;
            if (!files.TryGetValue(file, out var sections))
                return;
            if (!sections.TryGetValue(section, out var configs))
                return;
            int index = configs.FindIndex(x => x.Definition.Key == key);
            if (index < 0)
                return;
            var config = configs[index];
            config.SetSerializedValue(value);
            if (!ConfigMenu.activeInHierarchy)
                return;
            if (currentMod != mod)
                return;
            if (currentFile != file)
                return;
            if (!ConfigView.TryGetChild(x => MatchConfig(x, section, key), false, out GameObject go))
                return;
            go.GetComponent<UIConfigEntry>().Value.Render(value);
        }
        private static bool MatchConfig(GameObject go, string section, string key)
        {
            if (!go.TryGetComponent(out UIConfigEntry uentry))
                return false;
            var def = uentry.ConfigEntry.Definition;
            return def.Key == key && def.Section == section;
        }
        private void SendConfig(ConfigEntryBase config)
        {
            Debug.Log("Send config");
            Main.ConfigSync.SendConfigChange(currentMod, currentFile, config.Definition, config.GetSerializedValue());
        }
    }
}
