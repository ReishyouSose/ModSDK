using CoreLib.Data.Configuration;
using I2.Loc;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    public class UIConfigEntry : ButtonUIElement
    {
        public PugText Name;
        public SpriteRenderer SR;
        public SpriteRenderer Hover;
        public Transform Container;
        public PugText ServerValue;

        [HideInInspector]
        public UIConfigValueBox ValueBox;

        [HideInInspector]
        public ConfigEntryBase Entry;

        private ConfigScope scope;
        public void BindEntry(ConfigEntryBase entry, ConfigTemplate template, int hierarchy = 0)
        {
            Entry = entry;
            var def = entry.Definition;
            var key = def.Key;
            var local = entry.Description.Tags.FirstOrDefault(x => x is LocalizationOverride) is LocalizationOverride lfx ? lfx.Key :
                MiscHelper.GetLocalKey(entry.ConfigFile.ConfigFilePath, def.Section, key);
            name = "Entry " + key;
            Name.SetText(local, key);
            scope = entry.Scope;
            AdjustByHierarchy(hierarchy);
            Instantiate(scope.accessLevel switch
            {
                ConfigAccessLevel.Admin => template.Admin,
                ConfigAccessLevel.Server => template.Server,
                ConfigAccessLevel.Client => template.Client,
                _ => template.ViewOnly
            }, Container).localPosition = new(-0.5f, 0, 0);
            if (scope.requireReload)
                Instantiate(template.Reload, Container).localPosition = new(-1.5f, 0, 0);
            string desc = local + "Desc";
            if (LocalizationManager.TryGetTranslation(desc, out _))
            {
                showHoverDesc = true;
                optionalHoverDesc = new()
                {
                    mTerm = desc,
                };
            }
            MatchValue(template);
        }
        private void AdjustByHierarchy(int hierarchy)
        {
            float originalWidth = 23f;
            float targetRight = 11f;
            float newWidth = originalWidth - hierarchy;
            float x = targetRight - newWidth;

            if (TryGetComponent<WrapperUIComponent>(out var wrapper))
            {
                wrapper.renderWidthPixels = (int)(newWidth * 16);
            }

            Vector3 pos = transform.localPosition;
            pos.x = x;
            transform.localPosition = pos;

            pos = Container.localPosition;
            pos.x -= hierarchy;
            Container.localPosition = pos;

            x = newWidth / 2f;
            if (TryGetComponent<BoxCollider>(out var boxCollider))
            {
                Vector3 size = boxCollider.size;
                size.x = newWidth;
                boxCollider.size = size;

                Vector3 center = boxCollider.center;
                center.x = x;
                boxCollider.center = center;
            }

            void SetSR(SpriteRenderer sr)
            {
                Vector2 size = sr.size;
                size.x = newWidth;
                sr.size = size;

                var trans = sr.transform;
                Vector3 localPos = trans.localPosition;
                localPos.x = x;
                trans.localPosition = localPos;
            }
            SetSR(SR);
            SetSR(Hover);
        }
        private void Update()
        {
            var player = Manager.main.player;
            bool notEnterGame = player == null;
            bool allow = scope.accessLevel switch
            {
                ConfigAccessLevel.Admin => notEnterGame || player.adminPrivileges > 0,
                ConfigAccessLevel.Server => notEnterGame || (GeneralConfigMenuMod.config.AdminOnly.Value ? player.adminPrivileges > 0 : !player.guestMode),
                ConfigAccessLevel.Client => true,
                _ => false,
            };
            ValueBox.Editable = allow;
            ServerValue.gameObject.SetActive(!notEnterGame && scope.ShouldSync);
        }
        private void MatchValue(ConfigTemplate template)
        {
            if (Entry.SettingType == typeof(bool))
                ValueBox = Instantiate(template.Bool, Container);
            else
            {
                if (!TryMatchListType(template.List, out ValueBox))
                    ValueBox = Instantiate(template.Input, Container);
            }
            ValueBox.transform.localPosition = new(0, 0, 0);
            ValueBox.BindEntry(this);
        }
        public void ReceiveValue(string value)
        {
            ServerValue.formatFields[0] = value;
            ServerValue.Render();
            Debug.Log("Refresh");
            ValueBox.ReceiveValue(value);
        }
        public void OnMenuOpen()
        {
            if (Manager.main.player == null)
            {
                ServerValue.gameObject.SetActive(false);
            }
            if (scope.accessLevel is ConfigAccessLevel.Admin or ConfigAccessLevel.Server)
            {
                ServerValue.gameObject.SetActive(true);
            }
        }
        private bool TryMatchListType(UIConfigValueList list, out UIConfigValueBox box)
        {
            string[] accepts = null;
            if (Entry.SettingType.IsEnum)
            {
                var enums = Enum.GetValues(Entry.SettingType);
                accepts = new string[enums.Length];
                int index = 0;
                foreach (var value in enums)
                {
                    accepts[index++] = value.ToString();
                }
            }
            else if (MiscHelper.TryExtractAcceptableValues(Entry, out string[] values))
            {
                accepts = values;
            }
            if (accepts == null)
            {
                box = null;
                return false;
            }
            var valueList = Instantiate(list, Container);
            valueList.SetAccepts(accepts);
            box = valueList;
            return true;
        }
    }
}
