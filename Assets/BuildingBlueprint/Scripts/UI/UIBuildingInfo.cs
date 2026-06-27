using Assets.BuildingBlueprint.Scripts.Core;
using Assets.BuildingBlueprint.Scripts.Systems;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIBuildingInfo : MonoBehaviour
    {
        public UIBuildingName Input;
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
                if (!Input.inputIsActive)
                    return;
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
        public List<MaterialInfo> GetMaterails(out List<int> variations)
        {
            var player = Manager.main.player;
            variations = null;
            if (player == null)
                return null;
            variations = new();
            List<MaterialInfo> list = new();
            Dictionary<ObjectDataCD, int> requires = new();
            var ins = BuildingPlaceClient.Ins;
            foreach (var entities in Info.EntityInfos)
            {
                foreach (var entity in entities.Entities)
                {
                    bool zero = ins.AlwaysDropZero(entity.ObjectID, entity.Variation);
                    ObjectDataCD obj = new()
                    {
                        objectID = entity.ObjectID,
                        variation = zero ? 0 : entity.Variation,
                    };
                    requires.TryGetValue(obj, out int value);
                    requires[obj] = ++value;
                }
            }
            foreach (var tiles in Info.TileInfos)
            {
                foreach (var tile in tiles.Tiles)
                {
                    var tileObj = ins.TileToObject(tile);
                    bool zero = ins.AlwaysDropZero(tileObj.objectID, tileObj.variation);
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
                variations.Add(require.variation);
            }
            return list;
        }
    }
}
