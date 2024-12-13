using CoreLib.Data.Configuration;
using UnityEngine;

namespace Assets.GeneralConfigMenu.MonoBehaivours
{
    public class UIConfigEntry : MonoBehaviour
    {
        public PugText Label;
        public PugText Value;

        [HideInInspector]
        public ConfigEntryBase ConfigEntry;
    }
}
