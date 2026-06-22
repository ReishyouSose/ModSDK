using PugTilemap;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UITileTarget : ButtonUIElement
    {
        public TileType TileType;
        public GameObject Enable;
        public GameObject Disable;

        [HideInInspector]
        public bool State
        {
            get => state;
            set
            {
                state = value;
                Enable.SetActive(value);
                Disable.SetActive(!value);
            }
        }

        private bool state;

        protected override void Awake()
        {
            text.SetText(text.GetText() + TileType);
            Enable.SetActive(false);
            Disable.SetActive(true);
            base.Awake();
        }
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            State = !State;
            base.OnLeftClicked(mod1, mod2);
        }
        public override void OnSelected()
        {
            text.color = Color.yellow;
            base.OnSelected();
        }
        public override void OnDeselected(bool playEffect = true)
        {
            text.color = Color.white;
            base.OnDeselected(playEffect);
        }
    }
}
