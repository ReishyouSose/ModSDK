using Assets.GeneralConfigMenu.ConfigSync;
using CoreLib.Data.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(UIScrollWindow))]
    public class ModConfigMenu : RadicalMenu, IScrollable
    {
        internal static ModConfigMenu Instance { get; private set; }
        public Transform PageContainer;
        public Transform EmptryPage;
        public ConfigTemplate Template;
        public PugText Title;
        public PugText TitleShadow;
        public GameObject FullBar;

        private Transform filePage;
        private Transform current;
        private Transform currentContent;
        private LinearLayoutUIComponent layout;
        private UIScrollWindow scroll;
        private const string TITLE = "GeneralConfigMenu/Title";
        internal static readonly NativeQueue<ConfigDataRPC> Receive = new (Allocator.Persistent);

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            scroll = GetComponent<UIScrollWindow>();
            Template.gameObject.SetActive(false);
            filePage = Instantiate(EmptryPage, PageContainer);
            filePage.name = "File Page";
            var content = filePage.GetChild(0);
            var list = ConfigFile.AllConfigFilesReadOnly.ToList();
            foreach (var page in CombindConfigPage.PageList.Values)
            {
                var file = page.File;
                list.Remove(file);
                var path = MiscHelper.GetLocalKey(file.ConfigFilePath);
                RegisterFile(file, path, content).Detail = RegisterCombinePage(page, path);
            }
            foreach (var file in list)
            {
                var path = MiscHelper.GetLocalKey(file.ConfigFilePath);
                if (path.StartsWith("CoreLib"))
                    continue;
                RegisterFile(file, path, content).Detail = RegisterDetails(file, path);
            }
            SetCurrent(filePage);
        }
        public override void Activate()
        {
            base.Activate();
            scroll.ResetScroll();
        }
        public float GetCurrentWindowHeight()
        {
            return layout.GetUIComponentRenderHeight();
        }
        public bool IsTopElementSelected()
        {
            int count = currentContent.childCount;
            if (count <= 0)
                return false;
            return currentContent.GetChild(0) == Manager.ui.currentSelectedUIElement;
        }

        public bool IsBottomElementSelected()
        {
            int index = currentContent.childCount - 1;
            if (index <= 0)
                return false;
            return currentContent.GetChild(index) == Manager.ui.currentSelectedUIElement;
        }
        public void SwitchToDetail(string key, Transform view)
        {
            Title.SetText(key, key);
            TitleShadow.SetText(key, key);
            SetCurrent(view);
            Manager.menu.AttemptToPlayMenuSfx(SfxID.FIXME_menu_select, 0.6f, 0f, reuse: false);
        }
        public void SwitchToFile()
        {
            Title.localize = true;
            Title.Render(TITLE, false, true);
            TitleShadow.localize = true;
            TitleShadow.Render(TITLE, false, true);
            SetCurrent(filePage);
            AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f, false, 1f, 0f, true, true, 0f);
        }
        private UIConfigFile RegisterFile(ConfigFile configFile, string path, Transform content)
        {
            configFile.SaveOnConfigSet = false;
            var file = Instantiate(Template.File, content);
            file.gameObject.SetActive(true);
            file.GetComponentInChildren<PugText>().SetText(path, path);
            file.Key = path;
            return file;
        }
        private Transform RegisterDetails(ConfigFile file, string path)
        {
            var page = Instantiate(EmptryPage, PageContainer);
            page.gameObject.SetActive(false);
            page.name = path;
            var content = page.GetChild(0);
            Dictionary<string, List<ConfigEntryBase>> contents = new();
            foreach (var (def, entry) in file.Entries)
            {
                if (!contents.TryGetValue(def.Section, out var entries))
                    contents[def.Section] = entries = new();
                entries.Add(entry);
            }
            var sectionTemplate = Template.Section;
            foreach (var (section, entries) in contents)
            {
                var newSection = Instantiate(sectionTemplate, content);
                newSection.gameObject.SetActive(true);
                newSection.name = "Section " + section;
                newSection.Name.SetText(MiscHelper.GetLocalKey(path, section), section);
                foreach (var entry in entries)
                {
                    UIConfigEntry newEntry = Instantiate(Template.Entry, content);
                    newEntry.BindEntry(entry, Template, 1);
                    newSection.Entries.Add(newEntry);
                }
            }
            return page;
        }

        private Transform RegisterCombinePage(CombindConfigPage combind, string path)
        {
            var page = Instantiate(EmptryPage, PageContainer);
            page.gameObject.SetActive(false);
            page.name = path;
            var content = page.GetChild(0);
            Dictionary<string, List<ConfigData>> contents = new();
            foreach (var (entry, data) in combind.Configs)
            {
                var def = entry.Definition;
                if (!contents.TryGetValue(def.Section, out var datas))
                    contents[def.Section] = datas = new();
                datas.Add(data);
            }
            var sectionTemplate = Template.Section;
            foreach (var (section, datas) in contents)
            {
                var newSection = Instantiate(sectionTemplate, content);
                newSection.gameObject.SetActive(true);
                newSection.name = "Section " + section;
                newSection.Name.SetText(MiscHelper.GetLocalKey(path, section), section);
                foreach (var data in datas)
                {
                    UIConfigEntry newEntry = Instantiate(Template.Entry, content);
                    newEntry.BindEntry(data.Switch, Template, 1);
                    newSection.Entries.Add(newEntry);
                    var values = data.Values;
                    if (values == null)
                        continue;
                    foreach (var entry in data.Values.Values)
                    {
                        newEntry = Instantiate(Template.Entry, content);
                        newEntry.BindEntry(entry, Template, 2);
                        newSection.Entries.Add(newEntry);
                    }
                }
            }
            return page;
        }
        private void Update()
        {
            FullBar.SetActive(!scroll.scrollBar.gameObject.activeInHierarchy);
            while (Receive.TryDequeue(out ConfigDataRPC rpc))
            {
                TryReceiveSync(rpc.data.ToString());
            }
        }
        private void SetCurrent(Transform page)
        {
            if (current)
                current.gameObject.SetActive(false);
            page.gameObject.SetActive(true);
            scroll.scrollingContent = current = page;
            currentContent = page.GetChild(0);
            layout = currentContent.GetComponent<LinearLayoutUIComponent>();
            layout.RenderUIComponent(true);
            scroll.ResetScroll();
        }
        public void UpdateContainingElements(float scroll) { }
        public override bool OnCloseMenuRequest()
        {
            if (current != filePage)
            {
                SwitchToFile();
                return false;
            }
            return true;
        }
        public static void PlaySelectedSound()
        {
            Manager.menu.AttemptToPlayMenuSfx(SfxID.FIXME_menu_select, 1f, 0f, true);
        }
        public static bool TryConvertConfig(ConfigEntryBase config, out string data)
        {
            StringBuilder builder = new();
            builder.Append(MiscHelper.GetLocalKey(config.ConfigFile.ConfigFilePath)).Append('|');
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
        internal void TryReceiveSync(string data)
        {
            string[] info = data.Split('|');
            if (info.Length != 4)
            {
                Debug.Log("Try spilt " + data + " Failed");
                return;
            }
            string config = info[0], section = info[1], key = info[2], value = info[3];
            Transform page = null;
            foreach (Transform trans in PageContainer)
            {
                if (trans.name == config)
                {
                    page = trans;
                    break;
                }
            }
            if (!page)
            {
                Debug.Log(data + " can't find config page");
                return;
            }
            foreach (Transform go in page.GetChild(0))
            {
                if (go.TryGetComponent<UIConfigEntry>(out var uc))
                {
                    var def = uc.Entry.Definition;
                    if (def.Section == section && def.Key == key)
                    {
                        uc.ReceiveValue(value);
                        Debug.Log("Receive " + data);
                        return;
                    }
                }
            }
            Debug.Log(data + " can't find config entry in UI");
        }
    }
}