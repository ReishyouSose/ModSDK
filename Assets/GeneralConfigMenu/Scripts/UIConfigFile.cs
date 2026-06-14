using CoreLib.Data.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigFile : ButtonUIElement
    {
        [HideInInspector]
        public UIConfigPage Detail;

        [HideInInspector]
        public string Key;
    }
}
