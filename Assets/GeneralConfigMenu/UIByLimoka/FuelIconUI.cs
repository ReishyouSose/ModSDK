using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class FuelIconUI : UIelement
    {
        public GameObject hoverMarker;
        
        public bool isBurning;
        public float maxBurnTime;
        
        public SpriteRenderer fireIconRenderer;

        public void SetFillAmount(float value)
        {
            value = Math.Clamp(value, 0, 1) * 0.5f;
            fireIconRenderer.size = new Vector2(0.5f, value);
            fireIconRenderer.transform.localPosition = new Vector3(-0.03f, value * 0.5f - 0.25f, 0);
        }

        private void Awake()
        {
            hoverMarker.SetActive(false);
        }

        public override void OnDeselected(bool playEffect = true)
        {
            hoverMarker.SetActive(false);
        }

        public override void OnSelected()
        {
            hoverMarker.SetActive(true);
        }

        public override TextAndFormatFields GetHoverTitle()
        {
            return new TextAndFormatFields
            {
                text = "RailLogistics/BurnTitle",
            };
        }

        public override List<TextAndFormatFields> GetHoverDescription()
        {
            var text = isBurning ? "RailLogistics/BurnDescTime" : "RailLogistics/BurnDescNothing";
            
            return new List<TextAndFormatFields>
            {
                new TextAndFormatFields
                {
                    text = text,
                    formatFields = new[] { $"{maxBurnTime:F1}" }
                }
            };
            
        }
    }
}