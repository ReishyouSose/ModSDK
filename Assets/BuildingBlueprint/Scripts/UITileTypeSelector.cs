using PugTilemap;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UITileTypeSelector : SlotUIBase
    {
        public TileType TileType;
        public GameObject Enable;
        public GameObject Disable;
        public ObjectID Override;
        public int Amount;
        public int Variation;

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
        private int tileset;

        [HideInInspector]
        public int TileSet
        {
            get => tileset;
            set
            {
                tileset = value;
                if (!PugDatabase.objectDatasByTileTypeAndTileSet.TryGetValue(TileType == TileType.ground ? TileType.wall : TileType, out var sets))
                    return;
                if (!sets.TryGetValue(value, out var objData))
                    return;
                TileCD = new() { tileType = TileType, tileset = value };
                Contained = new()
                {
                    objectData = objData
                };
            }
        }
        public TileCD TileCD { get; private set; }
        public ContainedObjectsBuffer Contained
        {
            get => Override != ObjectID.None ? overrideContained : contained;
            set
            {
                if (Override != ObjectID.None)
                    return;
                contained = value;
                var info = PugDatabase.GetObjectInfo(value.objectID, value.variation);
                icon.sprite = info.icon;
                icon.transform.localPosition = info.iconOffset;
            }
        }
        private ContainedObjectsBuffer contained;
        private ContainedObjectsBuffer overrideContained;
        private void Start()
        {
            TileCD = new() { tileType = TileType, tileset = tileset };
            overrideContained = new()
            {
                objectData = new()
                {
                    objectID = Override,
                    variation = Variation,
                    amount = Amount
                }
            };
        }
        protected override ContainedObjectsBuffer GetSlotObject() => Contained;

        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            State = !State;
            base.OnLeftClicked(mod1, mod2);
        }
    }
}
