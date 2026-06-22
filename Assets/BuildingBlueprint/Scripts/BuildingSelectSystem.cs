using Pug.UnityExtensions;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class BuildingSelectSystem : PugSimulationSystemBase
    {
        internal static BuildingSelectSystem Ins { get; private set; }
        private EntityQuery query;
        private ComponentLookup<DirectionCD> directionLookup;
        private ComponentLookup<ObjectDataCD> objLookup;
        private ComponentLookup<LocalTransform> transLookup;
        private List<EntityCD> entities;
        private List<TileInfo> tiles;
        private float2? startPos;
        private float2 currentPos;
        private bool selecting;
        private TileAccessor tileAccessor;
        private BlueprintUI ui;
        protected override void OnCreate()
        {
            Ins = this;
            query = EntityManager.CreateEntityQuery(typeof(PlaceableCD));
            directionLookup = SystemAPI.GetComponentLookup<DirectionCD>();
            objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            entities = new();
            tiles = new();
            ui = BlueprintUI.Ins;
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            tileAccessor = CreateTileAccessor();
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            if (!selecting)
                return;
            var handler = ui.SelectHandler;
            switch (handler.Mode)
            {
                case SelectionMode.Box:
                    BoxCheck(handler.BoxOp);
                    break;
                case SelectionMode.Click:
                    ClickCheck(handler.ClickOp);
                    break;
            }
            base.OnUpdate();
        }
        public static float2 MouseWorld => EntityMonoBehaviour.ToWorldFromRender(Manager.ui.mouse.GetMouseGameViewPosition()).ToFloat2();
        public static Rect GetEntityRect(DirectionCD directionCD, float3 pos, int2 defaultTileSize)
        {
            var origin = pos.ToFloat2();
            float2 actualSize = directionCD.GetPrefabTileSize(defaultTileSize);
            origin -= actualSize / 2f;
            return new Rect(origin, actualSize);
        }

        public static bool Contains(Rect self, Rect other)
        {
            return self.xMin <= other.xMin &&
                   self.xMax >= other.xMax &&
                   self.yMin <= other.yMin &&
                   self.yMax >= other.yMax;
        }

        public void SetSelecing(bool state)
        {
            selecting = state;
        }

        private void BoxCheck(BoxOperator op)
        {
            if (!Manager.ui.currentSelectedUIElement && Input.GetMouseButtonDown(0))
            {
                startPos = MouseWorld;
                currentPos = startPos.Value;
                ui.StartSelect();
            }
            else if (startPos != null && Input.GetMouseButtonUp(0))
            {
                startPos = null;
                ui.EndSelect();
            }
            if (startPos != null)
            {
                var mouse = MouseWorld;
                var handler = ui.SelectHandler;
                if (!currentPos.Equals(mouse))
                {
                    currentPos = mouse;
                    float2 min = math.min(startPos.Value, mouse);
                    float2 max = math.max(startPos.Value, mouse);
                    Rect selectArea = new(min.x, min.y, max.x - min.x, max.y - min.y);
                    if (ui.SelectHandler.Layer == SelectionLayer.Entity)
                    {
                        var selected = this.entities;
                        selected.Clear();
                        var database = this.database;
                        using var entities = query.ToEntityArray(Allocator.Temp);
                        foreach (var entity in entities)
                        {
                            if (!objLookup.TryGetComponent(entity, out var obj))
                                continue;
                            ref var info = ref PugDatabase.GetEntityObjectInfo(obj.objectID, database, obj.variation);
                            directionLookup.TryGetComponent(entity, out var direction);
                            if (!transLookup.TryGetComponent(entity, out var trans))
                                continue;
                            var size = direction.GetPrefabTileSize(info.prefabTileSize);
                            var box = GetEntityRect(direction, trans.Position, size);
                            if (Contains(selectArea, box))
                            {
                                selected.Add(new EntityCD()
                                {
                                    ObjectID = obj.objectID,
                                    X = size.x,
                                    Y = size.y,
                                    Direction = direction.direction,
                                    Position = trans.Position,
                                    Variation = obj.variation,
                                });
                            }
                        }
                        ui.BoxOperate(selectArea, selected, op);
                    }
                    else
                    {
                        var selected = tiles;
                        selected.Clear();
                        int left = (int)math.ceil(min.x);
                        int right = (int)math.floor(max.x);
                        int bottom = (int)math.ceil(min.y);
                        int top = (int)math.floor(max.y);
                        var tileAccessor = this.tileAccessor;
                        var targets = handler.TileTarget;
                        for (int x = left; x <= right; x++)
                        {
                            for (int y = bottom; y <= top; y++)
                            {
                                int2 pos = new(x, y);
                                using var tiles = tileAccessor.Get(pos, Allocator.Temp);
                                TileInfo info = new() { Position = pos.ToVec2Int() };
                                var dict = info.Tiles = new();
                                foreach (var tile in tiles)
                                    dict.Add(tile, targets.Contains(tile.tileType));
                                selected.Add(info);
                            }
                        }
                        ui.BoxOperate(selectArea, selected, op);
                    }
                }
            }
        }

        private void ClickCheck(ClickOperator op)
        {
            bool hover = false;
            var mouse = MouseWorld;
            var handler = ui.SelectHandler;
            if (ui.SelectHandler.Layer == SelectionLayer.Entity)
            {
                var database = this.database;
                using var entities = query.ToEntityArray(Allocator.Temp);
                EntityInfo info = new()
                {
                    Position = mouse.ToFloat3(),
                    Entities = new()
                };
                foreach (var entity in entities)
                {
                    directionLookup.TryGetComponent(entity, out var direction);
                    if (!objLookup.TryGetComponent(entity, out var obj))
                        continue;
                    if (!transLookup.TryGetComponent(entity, out var trans))
                        continue;
                    var size = direction.GetPrefabTileSize(PugDatabase.GetEntityObjectInfo(obj.objectID, database, obj.variation).prefabTileSize);
                    var box = GetEntityRect(direction, trans.Position, size);
                    if (box.Contains(mouse))
                    {
                        hover = true;
                        info.Entities.Add(new EntityCD()
                        {
                            ObjectID = obj.objectID,
                            X = size.x,
                            Y = size.y,
                            Direction = direction.direction,
                            Position = trans.Position,
                            Variation = obj.variation,
                        });
                    }
                }
                ui.ClickOperate(hover, info, Input.GetMouseButtonDown(0), op);
            }
            else
            {
                var targets = handler.TileTarget;
                var pos = mouse.RoundToInt2();
                using var tiles = tileAccessor.Get(pos, Allocator.Temp);
                TileInfo info = new() { Position = pos.ToVec2Int() };
                var dict = info.Tiles = new();
                foreach (var tile in tiles)
                    dict.Add(tile, targets.Contains(tile.tileType));
                ui.ClickOperate(true, info, Input.GetMouseButtonDown(0), op);
            }
        }
    }
}
