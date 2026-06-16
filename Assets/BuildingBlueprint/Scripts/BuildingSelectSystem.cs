using Pug.UnityExtensions;
using PugTilemap;
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
        private List<BuildingInfo> selected;
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
            selected = new();
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
            else if (Input.GetMouseButtonUp(0))
            {
                startPos = null;
                ui.EndSelect();
            }
            if (startPos != null)
            {
                var mouse = MouseWorld;
                var selected = this.selected;
                if (!currentPos.Equals(mouse))
                {
                    currentPos = mouse;
                    selected.Clear();
                    var database = this.database;
                    using var entities = query.ToEntityArray(Allocator.Temp);
                    float2 min = math.min(startPos.Value, mouse);
                    float2 max = math.max(startPos.Value, mouse);
                    Rect selectArea = new(min.x, min.y, max.x - min.x, max.y - min.y);
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
                            selected.Add(new BuildingInfo()
                            {
                                ObjectID = obj.objectID,
                                X = size.x,
                                Y = size.y,
                                Direction = direction,
                                Position = trans.Position,
                                Variation = obj.variation,
                            });
                        }
                    }
                    ui.BoxOperate(selectArea, selected, op);
                    int left = (int)math.ceil(min.x);
                    int right = (int)math.floor(max.x);
                    int bottom = (int)math.ceil(min.y);
                    int top = (int)math.floor(max.y);

                    var tileAccessor = this.tileAccessor;
                    for (int x = left; x <= right; x++)
                    {
                        for (int y = bottom; y <= top; y++)
                        {
                            int2 pos = new(x, y);
                            using var tiles = tileAccessor.Get(pos, Allocator.Temp);
                            foreach(var tile in tiles)
                            {
                                Debug.Log(tile.tileType);
                            }
                        }
                    }
                }
            }
        }

        private void ClickCheck(ClickOperator op)
        {
            var mouse = MouseWorld;
            var database = this.database;
            using var entities = query.ToEntityArray(Allocator.Temp);
            BuildingInfo info = default;
            bool hover = false;
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
                    info = new BuildingInfo()
                    {
                        ObjectID = obj.objectID,
                        X = size.x,
                        Y = size.y,
                        Direction = direction,
                        Position = trans.Position,
                        Variation = obj.variation,
                    };
                    break;
                }
            }
            ui.ClickOperate(hover, info, Input.GetMouseButtonDown(0), op);
        }
    }
}
