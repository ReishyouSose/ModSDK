using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ConfigTemplate : MonoBehaviour
    {
        public UIConfigFile File;
        public UIConfigSection Section;
        public UIConfigEntry Entry;
        public UIConfigValueBool Bool;
        public UIConfigValueList List;
        public UIConfigValueInput Input;
        public UIPermissionButton Admin;
        public UIPermissionButton Server;
        public UIPermissionButton Client;
        public UIPermissionButton ViewOnly;
        public Transform Reload;
        public SpriteRenderer Icon;
    }
}
