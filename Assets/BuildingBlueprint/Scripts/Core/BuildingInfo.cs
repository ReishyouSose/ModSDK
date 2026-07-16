using Assets.BuildingBlueprint.Scripts.Systems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts.Core
{
    [Serializable]
    public class BuildingInfo
    {
        public string Name;
        public string Description;
        [JsonIgnore]
        public string Header;
        public List<EntityInfo> EntityInfos;
        public List<SerializeTileInfo> TileInfos;
        public int2 Size;
        private List<MaterialInfo> materials;
        private List<int> variations;

        public List<MaterialInfo> GetMaterails(out List<int> variations)
        {
            if (materials == null)
            {
                var player = Manager.main.player;
                variations = null;
                if (player == null)
                    return null;
                variations = new();
                List<MaterialInfo> list = new();
                Dictionary<ObjectDataCD, int> requires = new();
                var ins = BuildingPlaceClient.Ins;
                foreach (var entities in EntityInfos)
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
                foreach (var tiles in TileInfos)
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
                    list.Add(new(require.objectID, stack, BuildingPlaceClient.Ins.GetExistObjectAmount(player.entity, require.objectID, require.variation), Entity.Null, null));
                    variations.Add(require.variation);
                }
                materials = list;
                this.variations = variations;
            }
            variations = this.variations;
            return materials;
        }
    }
}
