using PugTilemap;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    [RequireComponent(typeof(PugText))]
    public class UITileTarget : ButtonUIElement
    {
        public TileType TileType;
        private new PugText text;
        public bool State { get; private set; }
        protected override void Awake()
        {
            text = GetComponent<PugText>();
            text.color = new(1, 1, 1, 0.5f);
            text.SetText(text.GetText() + TileType);
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
            text.color = new(1, 1, 1, State ? 1 : 0.5f);
            base.OnDeselected(playEffect);
        }
    }
}
