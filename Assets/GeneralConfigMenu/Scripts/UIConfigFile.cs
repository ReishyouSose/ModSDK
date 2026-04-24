using Assets.GeneralConfigMenu.Scripts.Vanilla;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigFile : ButtonUIElement
    {
        public ModConfigMenu Menu;

        [HideInInspector]
        public Transform Detail;

        [HideInInspector]
        public string Key;
        public void SwitchToDetail()
        {
            Menu.SwitchToDetail(Key, Detail);
        }
    }
}
