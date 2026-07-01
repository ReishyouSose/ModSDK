using Assets.BuildingBlueprint.Scripts.Core;
using Assets.BuildingBlueprint.Scripts.Systems;
using Pug.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
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
                var pos = entity.Position;
                var x = pos.x;
                var y = pos.y;
                foreach (var e in entity.Entities)
                {
                    var offset = new Vector2(e.X - 1, e.Y - 1) / 2f;
                    Vector2 size = new(e.X, e.Y);
                    var slot = GetOrCreateSlot(i++);
                    slot.sprite = Entity;
                    slot.size = size;
                    slot.transform.localPosition = new Vector3(x + offset.x, y + offset.y, 0);
                }
            }
            foreach (var tile in info.TileInfos)
            {
                var pos = tile.Position;
                var slot = GetOrCreateSlot(i++);
                slot.sprite = Tile;
                slot.size = Vector2.one;
                slot.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            }
            DeactiveExcessSlot(i);
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
