using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    [RequireComponent(typeof(ButtonUIElement))]
    public class UIBoxOperator : MonoBehaviour
    {
        public BoxOperator Op;
        private void Awake()
        {
            var button = GetComponent<ButtonUIElement>();
            button.showHoverTitle = true;
            button.optionalTitle = new()
            {
                mTerm = BuildingBlueprint.Key + $"Box/{Op}"
            };
        }
    }
}
