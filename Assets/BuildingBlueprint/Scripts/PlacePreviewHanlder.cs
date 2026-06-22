using Pug.UnityExtensions;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class PlacePreviewHanlder : MonoBehaviour
    {
        public Sprite Entity;
        public Sprite Tile;
        public SpriteRenderer Template;
        public Transform Camera;
        public Transform MarkContainer;
        private List<SpriteRenderer> marks;
        private BuildingInfo current;
        private void Awake()
        {
            marks = new()
            {
                Template
            };
        }
        public void RefreshPreview(BuildingInfo info)
        {
            int i = 0;
            current = info;
            foreach (var entity in info.EntityInfos)
            {
                foreach (var e in entity.Entities)
                {
                    var slot = GetOrCreateSlot(i++);
                    SetPosition(slot, e);
                }
            }
            foreach (var tile in info.TileInfos)
            {
                var slot = GetOrCreateSlot(i++);
                SetPosition(slot, tile);
            }
            DeactiveExcessSlot(i);
        }
        private void SetPosition(SpriteRenderer mark, EntityCD info)
        {
            Vector3 world = info.Position;
            var offset = new Vector2(info.X - 1, info.Y - 1) / 2f;
            mark.sprite = Entity;
            Vector2 size = new(info.X, info.Y);
            SetPosition(mark, world.x, world.z, offset, size);
        }
        private void SetPosition(SpriteRenderer mark, TileInfo info)
        {
            mark.sprite = Tile;
            var world = info.Position;
            SetPosition(mark, world.x, world.y, float2.zero, Vector2.one);
        }
        private void SetPosition(SpriteRenderer mark, float x, float y, float2 offset, Vector2 size)
        {
            mark.transform.localPosition = new Vector3(x + offset.x, y + offset.y, 0);
            mark.size = size;
        }

        private SpriteRenderer GetOrCreateSlot(int index)
        {
            if (index >= marks.Count)
                marks.Add(Instantiate(Template, MarkContainer));
            marks[index].gameObject.SetActive(true);
            return marks[index];
        }
        private void DeactiveExcessSlot(int index = 0)
        {
            for (int i = index; i < marks.Count; i++)
            {
                marks[i].gameObject.SetActive(false);
            }
        }
        private void Update()
        {
            var camera = -Manager.camera.smoothedCameraPosition;
            Camera.localPosition = new(camera.x, camera.z, 0);
            var mouse = EntityMonoBehaviour.ToWorldFromRender(Manager.ui.mouse.GetMouseGameViewPosition()).ToFloat2().RoundToInt2();
            MarkContainer.localPosition = new(mouse.x, mouse.y, 0);
            if (Manager.ui.currentSelectedUIElement || !Input.GetMouseButtonDown(0))
                return;
            BuildingPlaceClient.Ins.Place(mouse, current);
        }
    }
}
