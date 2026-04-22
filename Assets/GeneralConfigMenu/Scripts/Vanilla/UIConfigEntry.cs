using CoreLib.Data.Configuration;
using I2.Loc;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    [RequireComponent(typeof(BoxCollider))]
    public class UIConfigEntry : ButtonUIElement
    {
        public ConfigEntryBase Entry { get; private set; }
        public PugText Name;
        public SpriteRenderer SR;
        public SpriteRenderer Hover;
        public UIConfigValueBox Server;
        public UIConfigValueBox Client;
        public Transform IconContainer;
        public void BindEntry(ConfigEntryBase entry, ConfigTemplate template, int hierarchy = 0)
        {
            Entry = entry;
            var define = entry.Definition;
            var key = define.Key;
            var local = MiscHelper.GetLocalKey(entry.ConfigFile.ConfigFilePath, define.Section, key);
            name = "Entry " + key;
            Name.SetText(local, key);
            var scope = Entry.Scope;
            float y = 0f;
            AdjustByHierarchy(hierarchy);
            if (scope.requireReload)
            {
                Instantiate(template.Reload, IconContainer).localPosition = new(0, -0.5f, 0);
                y = 0.5f;
            }
            Instantiate(scope.accessLevel switch
            {
                ConfigAccessLevel.Admin => template.Admin,
                ConfigAccessLevel.Server => template.Server,
                ConfigAccessLevel.Client => template.Client,
                _ => template.ViewOnly
            }, IconContainer).localPosition = new(0, y, 0);
            string desc = local + "Desc";
            if (LocalizationManager.TryGetTranslation(desc, out _))
            {
                showHoverDesc = true;
                optionalHoverDesc = new()
                {
                    mTerm = desc,
                };
            }
        }
        private void AdjustByHierarchy(int hierarchy)
        {
            float originalWidth = 20f;
            float targetRight = 10f;
            float newWidth = originalWidth - hierarchy;
            float x = targetRight - newWidth;

            Vector3 pos = transform.localPosition;
            pos.x = x;
            transform.localPosition = pos;
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
    }
}
