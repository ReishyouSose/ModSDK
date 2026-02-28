using Assets.GeneralConfigMenu.RUIFramework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework.Extend
{
    public class RUIButtonGroup : MonoBehaviour
    {
        public List<RUIButton> Buttons { get; private set; } = new();
        public int MaxSelected = 1;
        private void OnValidate()
        {
            MaxSelected = math.max(1, MaxSelected);
        }
        private void OnTransformChildrenChanged()
        {
            Buttons.Clear();
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out RUIButton button))
                {
                    Buttons.Add(button);
                    button.ButtonGroup = this;
                }
            }
        }
    }
}
