using CoreLib.Submodule.UserInterface.Interface;
using Pug.UnityExtensions;
using PugTilemap;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class BlueprintUI : MonoBehaviour, IModUI
    {
        internal static BlueprintUI Ins { get; private set; }
        public SpriteRenderer SelectBorder;
        public SpriteRenderer MarkTemplate;
        public SpriteRenderer HoverMark;
        public Transform MarkContainer;
        public Transform Camera;
        public SelectHandler SelectHandler;
        public UIEntitySlot EntitySlot;
        public LinearLayoutUIComponent TileLayout;
        public Dictionary<TileType, UITileTypeSelector> TileSelectors;
        private List<SpriteRenderer> marks;
        private Dictionary<float3, BuildingInfo> entityPreview;
        private Dictionary<float3, BuildingInfo> entityRecord;
        private Dictionary<int2, TileInfo> tilePreview;
        private Dictionary<int2, TileInfo> tileRecord;
        public GameObject Root => SelectHandler.gameObject;
        private Dictionary<TileCD, bool> currentTile;
        public bool ShowWithPlayerInventory => false;

        public bool ShouldPlayerCraftingShow => false;
        private void Awake()
        {
            Ins = this;
            marks = new();
            MarkTemplate.gameObject.SetActive(false);
            SelectBorder.gameObject.SetActive(false);
            HoverMark.gameObject.SetActive(false);
            entityPreview = new();
            entityRecord = new();
            tilePreview = new();
            tileRecord = new();
            EntitySlot.gameObject.SetActive(false);
            TileLayout.gameObject.SetActive(false);
            HideUI();
        }
        private void Start()
        {
            TileSelectors = new();
            foreach (Transform t in TileLayout.transform)
            {
                if (t.TryGetComponent<UITileTypeSelector>(out var selector))
                    TileSelectors.Add(selector.TileType, selector);
            }
        }
        public void HideUI()
        {
            Root.SetActive(false);
            SetSelecting(false);
        }

        public void ShowUI()
        {
            Root.SetActive(true);
            SetSelecting(true);
        }

        public void SetSelecting(bool state)
        {
            BuildingSelectSystem.Ins?.SetSelecing(state);
        }

        public void StartSelect()
        {
            SelectBorder.gameObject.SetActive(true);
            SelectBorder.transform.localPosition = Vector3.zero;
            SelectBorder.size = Vector2.zero;
        }

        public void EndSelect()
        {
            SelectBorder.gameObject.SetActive(false);
            entityRecord = entityPreview.ToDictionary(x => x.Key, x => x.Value);
            tileRecord = tilePreview.ToDictionary(x => x.Key, x => x.Value);
        }
        public void BoxOperate(Rect rect, List<BuildingInfo> selected, BoxOperator op)
        {
            var s = selected.ToDictionary(x => x.Position);
            entityPreview = entityRecord.ToDictionary(x => x.Key, x => x.Value);
            switch (op)
            {
                case BoxOperator.New:
                    entityPreview = s;
                    break;
                case BoxOperator.Add:
                    foreach (var (k, v) in s)
                        entityPreview[k] = v;
                    break;
                case BoxOperator.Subtract:
                    foreach (var key in s.Keys)
                        entityPreview.Remove(key);
                    break;
                case BoxOperator.Intersect:
                    entityPreview = entityPreview.Where(kv => s.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
            }
            RefreshVisual(entityPreview);
            SelectBorder.transform.localPosition = new(rect.x, rect.y, 0);
            SelectBorder.size = rect.size;
        }
        public void BoxOperate(Rect rect, List<TileInfo> selected, BoxOperator op)
        {
            var s = selected.ToDictionary(x => x.Position);
            tilePreview = tileRecord.ToDictionary(x => x.Key, x => x.Value);
            switch (op)
            {
                case BoxOperator.New:
                    tilePreview = s;
                    break;
                case BoxOperator.Add:
                    foreach (var (k, v) in s)
                        tilePreview[k] = v;
                    break;
                case BoxOperator.Subtract:
                    foreach (var key in s.Keys)
                        tilePreview.Remove(key);
                    break;
                case BoxOperator.Intersect:
                    tilePreview = tilePreview.Where(kv => s.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
            }
            RefreshVisual(tilePreview);
            SelectBorder.transform.localPosition = new(rect.x, rect.y, 0);
            SelectBorder.size = rect.size;
        }

        public void ClickOperate(bool hover, BuildingInfo info, bool click, ClickOperator op)
        {
            if (!hover)
            {
                HoverMark.gameObject.SetActive(false);
                return;
            }
            if (click)
            {
                switch (op)
                {
                    case ClickOperator.Add:
                        entityRecord.TryAdd(info.Position, info);
                        break;
                    case ClickOperator.Remove:
                        entityRecord.Remove(info.Position);
                        break;
                }
                RefreshVisual(entityRecord);
                HoverMark.gameObject.SetActive(false);
                return;
            }
            if (!entityRecord.ContainsKey(info.Position))
            {
                HoverMark.gameObject.SetActive(true);
                SetPosition(HoverMark, info);
            }
        }
        public void ClickOperate(bool hover, TileInfo info, bool click, ClickOperator op)
        {
            if (!hover)
            {
                HoverMark.gameObject.SetActive(false);
                return;
            }
            if (click)
            {
                switch (op)
                {
                    case ClickOperator.Add:
                        tileRecord.TryAdd(info.Position, info);
                        break;
                    case ClickOperator.Remove:
                        tileRecord.Remove(info.Position);
                        break;
                }
                RefreshVisual(tileRecord);
                HoverMark.gameObject.SetActive(false);
                return;
            }
            if (!tileRecord.ContainsKey(info.Position))
            {
                HoverMark.gameObject.SetActive(true);
                SetPosition(HoverMark, info);
            }
        }
        private void RefreshVisual(Dictionary<float3, BuildingInfo> entities)
        {
            int i = 0;
            foreach (var info in entities.Values)
            {
                if (i >= marks.Count)
                {
                    var newMark = Instantiate(MarkTemplate, MarkContainer);
                    newMark.gameObject.SetActive(true);
                    marks.Add(newMark);
                }

                var mark = marks[i];
                mark.gameObject.SetActive(true);

                // 转换世界坐标到渲染坐标
                SetPosition(mark, info);
                i++;
            }

            // 隐藏多余的标记
            for (int j = i; j < marks.Count; j++)
            {
                if (marks[j] != null)
                    marks[j].gameObject.SetActive(false);
            }
        }
        private void RefreshVisual(Dictionary<int2, TileInfo> tiles)
        {
            int i = 0;
            foreach (var info in tiles.Values)
            {
                if (i >= marks.Count)
                {
                    var newMark = Instantiate(MarkTemplate, MarkContainer);
                    newMark.gameObject.SetActive(true);
                    marks.Add(newMark);
                }

                var mark = marks[i];
                mark.gameObject.SetActive(true);

                // 转换世界坐标到渲染坐标
                SetPosition(mark, info);
                i++;
            }

            // 隐藏多余的标记
            for (int j = i; j < marks.Count; j++)
            {
                if (marks[j] != null)
                    marks[j].gameObject.SetActive(false);
            }
        }
        public void SetPosition(SpriteRenderer mark, BuildingInfo info)
        {
            Vector3 world = info.Position;
            var offset = new Vector2(info.X - 1, info.Y - 1) / 2f;
            Vector2 size = new(info.X, info.Y);
            SetPosition(mark, world.x, world.z, offset, size);
        }
        public void SetPosition(SpriteRenderer mark, TileInfo info)
        {
            var world = info.Position;
            SetPosition(mark, world.x, world.y, float2.zero, Vector2.one);
        }
        private void SetPosition(SpriteRenderer mark, float x, float y, float2 offset, Vector2 size)
        {
            mark.GetComponent<RecordPosition>().Position = new(x, 0, y);
            mark.transform.localPosition = new Vector3(x + offset.x, y + offset.y, 0);
            mark.size = size;
        }

        private void Update()
        {
            var camera = -Manager.camera.smoothedCameraPosition;
            Camera.localPosition = new(camera.x, camera.z, 0);
            if (SelectHandler.Mode != SelectionMode.Check)
                return;
            bool hover = false;
            var mouse = EntityMonoBehaviour.ToWorldFromRender(Manager.ui.mouse.GetMouseGameViewPosition()).ToFloat2();
            bool click = Input.GetMouseButtonDown(0);
            foreach (var sr in marks)
            {
                if (!sr.gameObject.activeInHierarchy)
                    continue;
                var center = sr.transform.localPosition;
                var size = sr.size;
                Rect rect = new(center.x - size.x / 2f, center.y - size.y / 2f, size.x, size.y);
                if (!rect.Contains(mouse))
                    continue;
                hover = true;
                HoverMark.gameObject.SetActive(true);
                HoverMark.size = sr.size;
                HoverMark.transform.localPosition = sr.transform.localPosition;
                if (click)
                {
                    switch (SelectHandler.Layer)
                    {
                        case SelectionLayer.Entity:
                            CheckEntityInfo(entityRecord[sr.GetComponent<RecordPosition>()]);
                            break;
                        case SelectionLayer.Tile:
                            CheckTileInfo(tileRecord[sr.GetComponent<RecordPosition>()]);
                            break;
                    }
                }
                break;
            }
            if (!hover)
                HoverMark.gameObject.SetActive(false);
        }
        private void CheckTileInfo(TileInfo info)
        {
            EntitySlot.gameObject.SetActive(false);
            TileLayout.gameObject.SetActive(true);
            foreach (var (_, selector) in TileSelectors)
            {
                selector.gameObject.SetActive(false);
            }
            currentTile = info.Tiles;
            foreach (var (tile, state) in currentTile)
            {
                if (!TileSelectors.TryGetValue(tile.tileType, out var selector))
                    continue;
                selector.gameObject.SetActive(true);
                selector.TileSet = tile.tileset;
                selector.State = state;
            }
            TileLayout.RenderUIComponent(true);
        }
        private void CheckEntityInfo(BuildingInfo info)
        {
            TileLayout.gameObject.SetActive(false);
            EntitySlot.gameObject.SetActive(true);
            EntitySlot.Contained = new()
            {
                objectData = new()
                {
                    objectID = info.ObjectID,
                    variation = info.Variation,
                    amount = 1
                }
            };
        }
        public void ChangeTileState(UITileTypeSelector selector)
        {
            currentTile[selector.TileCD] = !currentTile[selector.TileCD];
        }
    }
}
