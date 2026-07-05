using Assets.BuildingBlueprint.Scripts.Core;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIBuildingInfo : MonoBehaviour
    {
        public UIBuildingName Input;
        public ButtonUIElement Select;
        public ButtonUIElement Delete;

        [HideInInspector]
        public BuildingInfo Info;
        public void Set(BuildingInfo info)
        {
            Info = info;
            Input.pugText.Render(info.Name, false, true);
        }
    }
}
