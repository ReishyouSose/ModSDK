using Assets.BuildingBlueprint.Scripts.Core;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIClickOperator : MonoBehaviour
    {
        public ClickOperator Op;
        private void Awake()
        {
            var button = GetComponent<ButtonUIElement>();
            button.showHoverTitle = true;
            button.optionalTitle = new()
            {
                mTerm = BuildingBlueprint.Key + $"Click/{Op}"
            };
        }
    }
}
