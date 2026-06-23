using Inventory;
using Pug.UnityExtensions;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public struct PlaceEntityRpc : IRpcCommand
    {
        public ObjectID ObjectID;
        public int Variation;
        public float3 Pos;
        public float3 Direction;
        public Entity Player;
    }

    public struct PlaceTileRpc : IRpcCommand
    {
        public TileCD Tile;
        public int2 Pos;
        public ObjectID ObjectID;
        public int Variation;
        public Entity Player;
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class BuildingPlaceClient : PugSimulationSystemBase
    {
        internal static BuildingPlaceClient Ins { get; private set; }
        private NativeQueue<PlaceEntityRpc> entities;
        private NativeQueue<PlaceTileRpc> tiles;
        private EntityArchetype entityArchetype;
        private EntityArchetype tileArchetype;
        protected override void OnCreate()
        {
            Ins = this;
            entities = new(Allocator.Persistent);
            tiles = new(Allocator.Persistent);
            entityArchetype = EntityManager.CreateArchetype(typeof(PlaceEntityRpc), typeof(SendRpcCommandRequest));
            tileArchetype = EntityManager.CreateArchetype(typeof(PlaceTileRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var entities = this.entities;
            var tiles = this.tiles;
            var entityArchetype = this.entityArchetype;
            var tileArchetype = this.tileArchetype;
            var ecb = CreateCommandBuffer();
            while (entities.TryDequeue(out var entity))
            {
                var e = ecb.CreateEntity(entityArchetype);
                ecb.SetComponent(e, entity);
            }
            while (tiles.TryDequeue(out var tile))
            {
                var e = ecb.CreateEntity(tileArchetype);
                ecb.SetComponent(e, tile);
            }
            base.OnUpdate();
        }
        public void Place(float2 mouse, BuildingInfo info)
        {
            var entitiesQueue = entities;
            var tilesQueue = tiles;
            var player = Manager.main.player.entity;
            Vector3 offset = mouse.ToFloat3();
            int2 offsetInt = mouse.RoundToInt2();
            foreach (var entities in info.EntityInfos)
            {
                foreach (var entity in entities.Entities)
                {
                    entitiesQueue.Enqueue(new()
                    {
                        ObjectID = entity.ObjectID,
                        Variation = entity.Variation,
                        Pos = entity.Position + offset,
                        Direction = entity.Direction,
                        Player = player,
                    });
                }
            }
            foreach (var tiles in info.TileInfos)
            {
                foreach (var tile in tiles.Tiles)
                {
                    var obj = TileToObject(tile);
                    tilesQueue.Enqueue(new()
                    {
                        ObjectID = obj.objectID,
                        Variation = obj.variation,
                        Tile = tile,
                        Pos = tiles.Position.ToInt2() + offsetInt,
                        Player = player
                    });
                }
            }
        }
        public static ObjectDataCD TileToObject(TileCD tile)
        {
            TileType type = tile.tileType;
            switch (tile.tileType)
            {
                case TileType.ground:
                    type = TileType.wall;
                    break;
                case TileType.roofHole:
                    return new()
                    {
                        objectID = ObjectID.RoofingTool,
                        variation = 0
                    };
                case TileType.water:
                    return new()
                    {
                        objectID = ObjectID.Bucket,
                        variation = tile.tileset + 1
                    };
            }
            if (!PugDatabase.objectDatasByTileTypeAndTileSet.TryGetValue(type, out var sets))
                return default;
            if (!sets.TryGetValue(tile.tileset, out var objData))
                return default;
            return objData;
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BuildingPlaceServer : PugSimulationSystemBase
    {
        private ComponentLookup<PlaceEntityRpc> entityLookup;
        private ComponentLookup<PlaceTileRpc> tileLookup;
        private InventoryHandlerShared shared;
        private ComponentLookup<AlwaysDropVariationZeroCD> zeroLookup;
        protected override void OnCreate()
        {
            NeedDatabase();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<TileUpdateBuffer>();
            RequireForUpdate<InventoryChangeBuffer>();
            RequireForUpdate<SkillTalentsTableCD>();
            RequireForUpdate<UpgradeCostsTableCD>();
            RequireForUpdate<InventoryAuxDataSystemDataCD>();
            RequireForUpdate<NetworkTime>();
            entityLookup = SystemAPI.GetComponentLookup<PlaceEntityRpc>();
            tileLookup = SystemAPI.GetComponentLookup<PlaceTileRpc>();
            zeroLookup = SystemAPI.GetComponentLookup<AlwaysDropVariationZeroCD>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            shared = new(ref CheckedStateRef, SystemAPI.GetSingleton<PugDatabase.DatabaseBankCD>(), SystemAPI.GetSingleton<SkillTalentsTableCD>(),
                SystemAPI.GetSingleton<UpgradeCostsTableCD>(), SystemAPI.GetSingleton<InventoryAuxDataSystemDataCD>());
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var entityLookup = this.entityLookup;
            var tileLookup = this.tileLookup;
            var zeroLookup = this.zeroLookup;
            var shared = this.shared;
            shared.Update(ref CheckedStateRef, CreateCommandBuffer(), SystemAPI.GetSingleton<NetworkTime>());
            var invChange = SystemAPI.GetSingletonBuffer<InventoryChangeBuffer>();
            var tileChange = SystemAPI.GetSingletonBuffer<TileUpdateBuffer>();
            var containedObjectsBufferLookup = shared.containedObjectsBufferLookup;
            bool creative = WorldInfo.IsWorldModeEnabled(WorldMode.Creative);
            var database = this.database;
            Entities.ForEach((Entity e) =>
            {
                shared.ecb.DestroyEntity(e);
                if (entityLookup.TryGetComponent(e, out var entity))
                {
                    if (!containedObjectsBufferLookup.TryGetBuffer(entity.Player, out var inv))
                    {
                        return;
                    }
                    var objectID = entity.ObjectID;
                    var primary = PugDatabase.GetPrimaryPrefabEntity(objectID, database);
                    var variation = entity.Variation;
                    var findVari = zeroLookup.HasComponent(primary) ? 0 : variation;
                    for (int i = 0; i < inv.Length; i++)
                    {
                        var slot = inv[i];
                        if (slot.objectID == objectID && slot.variation == findVari)
                        {
                            if (slot.amount > 0)
                            {
                                shared.isFirstTimeFullyPredictingTick = true;
                                InventoryUtility.ConsumeEntityAt(shared, entity.Player, i, 1, false, entity.Pos, variation, entity.Direction);
                                return;
                            }
                        }
                    }
                }
                else if (tileLookup.TryGetComponent(e, out var tile))
                {
                    var objectID = tile.ObjectID;
                    if (objectID is ObjectID.None or ObjectID.Bucket)
                    {
                        var tileCD = tile.Tile;
                        EntityUtility.AddTile(tileCD.tileset, tileCD.tileType, tile.Pos, creative, tileChange);
                        return;
                    }
                    if (!containedObjectsBufferLookup.TryGetBuffer(tile.Player, out var inv))
                    {
                        return;
                    }
                    var variation = tile.Variation;
                    for (int i = 0; i < inv.Length; i++)
                    {
                        var slot = inv[i];
                        if (slot.objectID == objectID && slot.variation == variation)
                        {
                            var amount = slot.amount;
                            if (amount > 0)
                            {
                                var tileCD = tile.Tile;
                                EntityUtility.AddTile(tileCD.tileset, tileCD.tileType, tile.Pos, creative, tileChange);
                                inv[i] = amount == 1 ? default : new()
                                {
                                    auxDataIndex = slot.auxDataIndex,
                                    objectData = new()
                                    {
                                        objectID = objectID,
                                        variation = variation,
                                        amount = amount - 1,
                                    }
                                };
                                return;
                            }
                        }
                    }
                }
            })
                .WithName("PlaceBuildingSystem")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithAny<PlaceEntityRpc, PlaceTileRpc>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
