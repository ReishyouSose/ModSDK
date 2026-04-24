using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class ConfigTemplate : MonoBehaviour
    {
        public UIConfigFile File;
        public UISection Section;
        public UIConfigEntry Entry;
        public UIConfigValueBool Bool;
        public UIConfigValueList List;
        public UIConfigValueInput Input;
        public Transform Admin;
        public Transform Server;
        public Transform Client;
        public Transform ViewOnly;
        public Transform Reload;
    }
}
