using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Components
{
    [RequireComponent(typeof(InventoryAuthoring))]
    public class ModSlotRequireAuthoring : MonoBehaviour
    {
        [Serializable]
        public class ModSlotRequire
        {
            public int RequireIndex;
            public int ReplaceIndex;
            public string Name;
        }
        public List<ModSlotRequire> slotRequirements = new();
    }
}
