using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    public class ConfigTemplate : MonoBehaviour
    {
        public UIConfigFile File;
        public UISection Section;
        public UIConfigEntry Entry;
        public UIConfigValueBox Bool;
        public UIConfigValueBox List;
        public UIConfigValueBox Input;
        public Transform Admin;
        public Transform Server;
        public Transform Client;
        public Transform ViewOnly;
        public Transform Reload;
    }
}
