using PugTilemap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class SelectHandler : MonoBehaviour
    {
        public GameObject EntityIcon;
        public GameObject TileIcon;
        public Transform ModeMark;
        public Transform BoxMark;
        public Transform ClickMark;
        public GameObject BoxContainer;
        public GameObject ClickContainer;
        public GameObject TileFilterButton;
        public GameObject TileFilterContainer;
        public LinearLayoutUIComponent FilterLayout;
        public UITileTarget FilterTemplate;
        public List<TileType> FilterTypes;

        [HideInInspector]
        public HashSet<TileType> ExceptTiles;
        [HideInInspector]
        public SelectionLayer Layer;
        [HideInInspector]
        public SelectionMode Mode;
        [HideInInspector]
        public BoxOperator BoxOp;
        [HideInInspector]
        public ClickOperator ClickOp;
        [HideInInspector]
        public HashSet<TileType> TileTarget;

        private bool rendered;

        private void Awake()
        {
            BoxContainer.SetActive(false);
            ClickContainer.SetActive(false);
            TileTarget = new();
            Mode = SelectionMode.Check;
            BoxContainer.transform.localPosition = new(0, -1.25f, 0);
            ClickContainer.transform.localPosition = new(0, -1.25f, 0);
            TileFilterButton.SetActive(false);
            TileFilterContainer.SetActive(false);
            FilterTemplate.gameObject.SetActive(false);
            var container = TileFilterContainer.GetComponent<UIScrollWindow>().scrollingContent.GetChild(0);
            foreach (var filter in FilterTypes)
            {
                var f = Instantiate(FilterTemplate, container);
                f.TileType = filter;
                f.gameObject.SetActive(true);
            }
            Array array = Enum.GetValues(typeof(TileType));
            ExceptTiles = new();
            foreach (var f in array)
                ExceptTiles.Add((TileType)f);
            foreach (var f in FilterTypes)
            {
                ExceptTiles.Remove(f);
            }
        }
        public void ChangeSelectionLayer()
        {
            if (Layer == SelectionLayer.Tile)
            {
                Layer = SelectionLayer.Entity;
                EntityIcon.SetActive(true);
                TileIcon.SetActive(false);
                BoxContainer.transform.localPosition = new(0, -1.25f, 0);
                ClickContainer.transform.localPosition = new(0, -1.25f, 0);
                TileFilterButton.SetActive(false);
                TileFilterContainer.SetActive(false);
                return;
            }
            EntityIcon.SetActive(false);
            TileIcon.SetActive(true);
            Layer = SelectionLayer.Tile;
            BoxContainer.transform.localPosition = new(-1.25f, -1.25f, 0);
            ClickContainer.transform.localPosition = new(-1.25f, -1.25f, 0);
            TileFilterButton.SetActive(true);
        }
        public void ChangeSelectionMode(GameObject button)
        {
            ModeMark.position = button.transform.position;
            var mode = button.GetComponent<UISelectionMode>().Mode;
            Mode = mode;
            switch (mode)
            {
                case SelectionMode.Box:
                    BoxContainer.SetActive(true);
                    ClickContainer.SetActive(false);
                    break;
                case SelectionMode.Click:
                    ClickContainer.SetActive(true);
                    BoxContainer.SetActive(false);
                    break;
                default:
                    ClickContainer.SetActive(false);
                    BoxContainer.SetActive(false);
                    break;
            }
        }

        public void ChangeBoxOp(GameObject button)
        {
            BoxMark.position = button.transform.position;
            BoxOp = button.GetComponent<UIBoxOperator>().Op;
        }

        public void ChangeClickOp(GameObject button)
        {
            ClickMark.position = button.transform.position;
            ClickOp = button.GetComponent<UIClickOperator>().Op;
        }

        public void ChangeTileFilter(UITileTarget button)
        {
            var target = button.TileType;
            Debug.Log(target);
            if (button.State)
                TileTarget.Add(target);
            else
                TileTarget.Remove(target);
        }
        public void SwitchTileFilter()
        {
            TileFilterContainer.SetActive(!TileFilterContainer.activeSelf);
            if (rendered)
                return;
            rendered = true;
            FilterLayout.RenderUIComponent(true);
        }
    }
}
