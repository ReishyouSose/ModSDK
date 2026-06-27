using PugTilemap;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UITileTarget : ButtonUIElement
    {
        public GameObject Enable;
        public GameObject Disable;

        [HideInInspector]
        public TileType TileType;
        [HideInInspector]
        public int Index;

        protected override void Awake()
        {
            text.SetText(text.GetText() + TileType);
            Enable.SetActive(false);
            Disable.SetActive(true);
            base.Awake();
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
