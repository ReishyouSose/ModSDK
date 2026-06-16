using CoreLib.Submodule.UserInterface.Interface;
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
        private List<SpriteRenderer> marks;
        private Dictionary<float3, BuildingInfo> preview;
        private Dictionary<float3, BuildingInfo> record;
        public GameObject Root => SelectHandler.gameObject;
        public bool ShowWithPlayerInventory => false;

        public bool ShouldPlayerCraftingShow => false;
        private bool open;
        private void Awake()
        {
            Ins = this;
            marks = new();
            MarkTemplate.gameObject.SetActive(false);
            SelectBorder.gameObject.SetActive(false);
            HoverMark.gameObject.SetActive(false);
            preview = new();
            record = new();
            HideUI();
        }
        public void Switch()
        {
            open = !open;
            if (open)
                ShowUI();
            else
                HideUI();
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
            record = preview.ToDictionary(x => x.Key, x => x.Value);
        }
        public void BoxOperate(Rect rect, List<BuildingInfo> selected, BoxOperator op)
        {
            var s = selected.ToDictionary(x => x.Position);
            preview = record.ToDictionary(x => x.Key, x => x.Value);
            switch (op)
            {
                case BoxOperator.New:
                    preview = s;
                    break;
                case BoxOperator.Add:
                    foreach (var (k, v) in s)
                        preview[k] = v;
                    break;
                case BoxOperator.Subtract:
                    foreach (var key in s.Keys)
                        preview.Remove(key);
                    break;
                case BoxOperator.Intersect:
                    preview = preview.Where(kv => s.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
            }
            RefreshVisual(preview);
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
                        record.TryAdd(info.Position, info);
                        break;
                    case ClickOperator.Remove:
                        record.Remove(info.Position);
                        break;
                }
                RefreshVisual(record);
                HoverMark.gameObject.SetActive(false);
                return;
            }
            if (!record.ContainsKey(info.Position))
            {
                HoverMark.gameObject.SetActive(true);
                SetPosition(HoverMark, info);
            }
        }
        private void RefreshVisual(Dictionary<float3, BuildingInfo> buildings)
        {
            int i = 0;
            foreach (var info in buildings.Values)
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
        private void SetPosition(SpriteRenderer mark, BuildingInfo info)
        {
            Vector3 world = info.Position;
            var offset = new float2(info.X - 1, info.Y - 1) / 2f;
            mark.transform.localPosition = new Vector3(world.x + offset.x, world.z + offset.y, 0);
            mark.size = new Vector2(info.X, info.Y);
        }

        private void Update()
        {
            var camera = -Manager.camera.smoothedCameraPosition;
            Camera.localPosition = new(camera.x, camera.z, 0);
        }
    }
}
