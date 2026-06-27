using Assets.BuildingBlueprint.Scripts.Components;
using Assets.BuildingBlueprint.Scripts.Core;
using Inventory;
using Pug.UnityExtensions;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.BuildingBlueprint.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class BuildingPlaceClient : PugSimulationSystemBase
    {
        internal static BuildingPlaceClient Ins { get; private set; }
        private NativeQueue<PlaceEntityRpc> entities;
        private NativeQueue<PlaceTileRpc> tiles;
        private EntityArchetype entityArchetype;
        private EntityArchetype tileArchetype;
        private TileWithTilesetToObjectDataMapCD tileSetMap;
        private ComponentLookup<AlwaysDropVariationZeroCD> zeroLookup;
        protected override void OnCreate()
        {
            Ins = this;
            entities = new(Allocator.Persistent);
            tiles = new(Allocator.Persistent);
            entityArchetype = EntityManager.CreateArchetype(typeof(PlaceEntityRpc), typeof(SendRpcCommandRequest));
            tileArchetype = EntityManager.CreateArchetype(typeof(PlaceTileRpc), typeof(SendRpcCommandRequest));
            zeroLookup = SystemAPI.GetComponentLookup<AlwaysDropVariationZeroCD>();
            RequireForUpdate<TileWithTilesetToObjectDataMapCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var entities = this.entities;
            var tiles = this.tiles;
            var entityArchetype = this.entityArchetype;
            var tileArchetype = this.tileArchetype;
            var ecb = CreateCommandBuffer();
            tileSetMap = SystemAPI.GetSingleton<TileWithTilesetToObjectDataMapCD>();
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
        public void Place(int2 mouse, BuildingInfo info)
        {
            var entitiesQueue = entities;
            var tilesQueue = tiles;
            var player = Manager.main.player.entity;
            foreach (var entities in info.EntityInfos)
            {
                foreach (var entity in entities.Entities)
                {
                    entitiesQueue.Enqueue(new()
                    {
                        ObjectID = entity.ObjectID,
                        Variation = entity.Variation,
                        Pos = entities.Position + mouse,
                        Direction = entity.Direction,
                        Color = entity.Color,
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
                        Pos = tiles.Position + mouse,
                        Player = player
                    });
                }
            }
        }
        public ObjectDataCD TileToObject(TileCD tile)
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
            return PugDatabase.TryGetTileItemInfo(type, (Tileset)tile.tileset, tileSetMap);
        }
        public bool AlwaysDropZero(ObjectID id, int variation)
        {
            return zeroLookup.HasComponent(PugDatabase.GetPrimaryPrefabEntity(id, database, variation));
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BuildingPlaceServer : PugSimulationSystemBase
    {
        private ComponentLookup<PlaceEntityRpc> entityLookup;
        private ComponentLookup<PlaceTileRpc> tileLookup;
        private ComponentLookup<AlwaysDropVariationZeroCD> zeroLookup;
        private BufferLookup<ContainedObjectsBuffer> containedLookup;
        protected override void OnCreate()
        {
            NeedDatabase();
            RequireForUpdate<PugDatabase.DatabaseBankCD>();
            RequireForUpdate<TileUpdateBuffer>();
            RequireForUpdate<InventoryChangeBuffer>();
            entityLookup = SystemAPI.GetComponentLookup<PlaceEntityRpc>();
            tileLookup = SystemAPI.GetComponentLookup<PlaceTileRpc>();
            zeroLookup = SystemAPI.GetComponentLookup<AlwaysDropVariationZeroCD>();
            containedLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var entityLookup = this.entityLookup;
            var tileLookup = this.tileLookup;
            var zeroLookup = this.zeroLookup;
            var containedLookup = this.containedLookup;
            var ecb = CreateCommandBuffer();
            var invChange = SystemAPI.GetSingletonBuffer<InventoryChangeBuffer>();
            var tileChange = SystemAPI.GetSingletonBuffer<TileUpdateBuffer>();
            bool creative = WorldInfo.IsWorldModeEnabled(WorldMode.Creative);
            var database = this.database;
            Entities.ForEach((Entity e) =>
            {
                ecb.DestroyEntity(e);
                if (entityLookup.TryGetComponent(e, out var entity))
                {
                    if (!containedLookup.TryGetBuffer(entity.Player, out var inv))
                    {
                        return;
                    }
                    var objectID = entity.ObjectID;
                    var primary = PugDatabase.GetPrimaryPrefabEntity(objectID, database);
                    var variation = entity.Variation;
                    var findVari = zeroLookup.HasComponent(primary) ? 0 : variation;
                    int index = -1;
                    if (!creative)
                    {
                        for (int i = 0; i < inv.Length; i++)
                        {
                            var slot = inv[i];
                            if (slot.objectID == objectID && slot.variation == findVari)
                            {
                                var amount = slot.amount;
                                if (amount > 0)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }
                    }
                    if (creative || index > -1)
                    {
                        var create = EntityUtility.CreateEntity(ecb, entity.Pos.ToFloat3(), objectID, 1, database, findVari);
                        if (!entity.Direction.Equals(int2.zero))
                        {
                            ecb.SetComponent(create, new DirectionCD() { direction = entity.Direction.ToFloat3() });
                        }
                        if (entity.Color != PaintableColor.Unpainted)
                        {
                            ecb.SetComponent(create, new PaintableObjectCD() { color = entity.Color });
                        }
                        if (creative)
                            return;
                        invChange.Add(new()
                        {
                            inventoryChangeData = Create.ConsumeEntityAt(entity.Player, index, 1, true, false)
                        });
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
                    if (!containedLookup.TryGetBuffer(tile.Player, out var inv))
                    {
                        return;
                    }
                    var primary = PugDatabase.GetPrimaryPrefabEntity(objectID, database);
                    var variation = tile.Variation;
                    var findVari = zeroLookup.HasComponent(primary) ? 0 : variation;
                    int index = -1;
                    if (!creative)
                    {
                        for (int i = 0; i < inv.Length; i++)
                        {
                            var slot = inv[i];
                            if (slot.objectID == objectID && slot.variation == findVari)
                            {
                                var amount = slot.amount;
                                if (amount > 0)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }
                    }
                    if (creative || index > -1)
                    {
                        var tileCD = tile.Tile;
                        EntityUtility.AddTile(tileCD.tileset, tileCD.tileType, tile.Pos, creative, tileChange);
                        if (creative)
                            return;
                        invChange.Add(new()
                        {
                            inventoryChangeData = Create.ConsumeEntityAt(tile.Player, index, 1, true, false)
                        });
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
