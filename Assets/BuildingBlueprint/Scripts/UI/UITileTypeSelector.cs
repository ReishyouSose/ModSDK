using PugTilemap;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UITileTypeSelector : ButtonUIElement
    {
        public GameObject Enable;
        public GameObject Disable;
        public GameObject Invalid;
        public SpriteMask Mask;
        public UISingleSlot Slot;

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
        public TileCD TileCD { get; private set; }

        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            State = !State;
            base.OnLeftClicked(mod1, mod2);
        }
        public override TextAndFormatFields GetHoverTitle()
        {
            return new()
            {
                text = "BuildingBlueprint_TileType/" + TileCD.tileType
            };
        }

        public void Set(TileCD tile, int index, bool except, bool state)
        {
            TileCD = tile;
            TileType type = tile.tileType;
            Slot.icon.sortingOrder = index;
            Mask.frontSortingOrder = index;
            Mask.backSortingOrder = index - 1;
            State = state;
            if (except)
            {
                showHoverDesc = true;
                canBeClicked = false;
                Disable.SetActive(false);
                Invalid.SetActive(true);
            }
            else
            {
                showHoverDesc = false;
                canBeClicked = true;
                Invalid.SetActive(false);
            }
            switch (tile.tileType)
            {
                case TileType.ground:
                    type = TileType.wall;
                    break;
                case TileType.roofHole:
                    Slot.Contained = new()
                    {
                        objectData = new()
                        {
                            objectID = ObjectID.RoofingTool,
                            amount = 200,
                            variation = 0
                        }
                    };
                    return;
                case TileType.water:
                    Slot.Contained = new()
                    {
                        objectData = new()
                        {
                            objectID = ObjectID.Bucket,
                            amount = 1,
                            variation = tile.tileset + 1
                        }
                    };
                    return;
                case TileType.dugUpGround:
                    Slot.Contained = new()
                    {
                        objectData = new()
                        {
                            objectID = ObjectID.WoodHoe,
                            amount = 50,
                            variation = 0
                        }
                    };
                    return;
            }
            if (!PugDatabase.objectDatasByTileTypeAndTileSet.TryGetValue(type, out var sets))
                return;
            if (!sets.TryGetValue(tile.tileset, out var objData))
                return;
            Slot.Contained = new()
            {
                objectData = objData
            };
        }
    }
}
