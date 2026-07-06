using Assets.BuildingBlueprint.Scripts.Core;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIBuildingInfo : MonoBehaviour
    {
        public UIBuildingName Input;

        [HideInInspector]
        public BuildingInfo Info;
    }
}
