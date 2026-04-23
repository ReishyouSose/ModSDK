using CoreLib.Data.Configuration;
using I2.Loc;
using PugMod;
using System.Linq;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    [RequireComponent(typeof(BoxCollider))]
    public class UIConfigEntry : ButtonUIElement
    {
        public PugText Name;
        public SpriteRenderer SR;
        public SpriteRenderer Hover;
        public Transform Container;

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
            bool allow = scope.accessLevel switch
            {
                ConfigAccessLevel.Admin => player == null || player.adminPrivileges > 0,
                ConfigAccessLevel.Server => player == null || !player.guestMode,
                ConfigAccessLevel.ViewOnly => false,
                _ => true,
            };
            ValueBox.Editable = allow;
        }
        private void MatchValue(ConfigTemplate template)
        {
            ValueBox = Instantiate(template.Bool, Container);
            ValueBox.transform.localPosition = new(0, 0, 0);
        }
        public void SetValue(string value)
        {

        }
    }
}
