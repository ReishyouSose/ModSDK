using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UIBuildingInfo : MonoBehaviour
    {
        public TextInputField Input;
        public ButtonUIElement Select;
        public ButtonUIElement Delete;

        [HideInInspector]
        public BuildingInfo Info;

        [HideInInspector]
        public int Index;

        private void Awake()
        {
            Input.onInputFieldDone.AddListener(() =>
            {
                var text = Input.pugText;
                if (text.GetTextLength() == 0)
                {
                    text.Render(Index.ToString(), false, true);
                }
                Info.Name = text.GetText();
                BlueprintUI.Ins.SaveToFile();
            });
        }
        public void Set(BuildingInfo info, int index)
        {
            Info = info;
            Index = index;
            Input.pugText.Render(info.Name, false, true);
        }
        public List<MaterialInfo> GetMaterails()
        {
            var player = Manager.main.player;
            if (player == null)
                return null;
            List<MaterialInfo> list = new();
            Dictionary<ObjectDataCD, int> requires = new();
            foreach (var entities in Info.EntityInfos)
            {
                foreach (var entity in entities.Entities)
                {
                    ObjectDataCD obj = new()
                    {
                        objectID = entity.ObjectID,
                        variation = entity.Variation,
                    };
                    requires.TryGetValue(obj, out int value);
                    requires[obj] = ++value;
                }
            }
            foreach (var tiles in Info.TileInfos)
            {
                foreach (var tile in tiles.Tiles.Keys)
                {
                    var tileObj = BuildingPlaceClient.TileToObject(tile);
                    ObjectDataCD obj = new()
                    {
                        objectID = tileObj.objectID,
                        variation = tileObj.variation,
                    };
                    requires.TryGetValue(obj, out int value);
                    requires[obj] = ++value;
                }
            }
            foreach (var (require, stack) in requires)
            {
                list.Add(new(require.objectID, stack, player.playerInventoryHandler.GetExistingAmountOfObject(require.objectID), Entity.Null, null));
            }
            return list;
        }
    }
}
