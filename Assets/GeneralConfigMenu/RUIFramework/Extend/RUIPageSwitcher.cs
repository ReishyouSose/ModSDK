using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework.Extend
{
    public class RUIPageSwitcher : MonoBehaviour
    {
        private struct Local
        {
            public int page;
            public int row;
            public int col;
            public readonly override string ToString()
            {
                StringBuilder sb = new();
                sb.Append('{').Append("Page: ").Append(page)
                    .Append(" Row: ").Append(row).Append(" Column: ")
                    .Append(col).Append('}');
                return sb.ToString();
            }
        }
        public RUIButton PageLeft;
        public RUIButton PageRight;
        public PugText PageText;
        public PugText PageTextOutLine;
        public int MaxRow;
        public int MaxColumn;
        public Vector2 Spacing;
        public bool HorizenSort;
        public Transform Template;
        public int CurrentPage { get; private set; } = 0;
        public int MaxPage { get; private set; } = 1;
        private int indexPage = 1;
        private int indexRow = 1;
        private int indexColumn = 1;
        private readonly Dictionary<Local, Transform> children = new();
        private void OnValidate()
        {
            MaxRow = math.max(MaxRow, 1);
            MaxColumn = math.max(MaxColumn, 1);
        }
        private void Start()
        {
            PageLeft.AddEvent(RMouseEventType.LeftDown, OnPageLeftClick);
            PageRight.AddEvent(RMouseEventType.LeftDown, OnPageRightClick);
        }
        private void ResetPage()
        {
            foreach (var (_, child) in children)
            {
                Destroy(child.gameObject);
            }
            children.Clear();
            MaxPage = 1;
            indexPage = 1;
            indexRow = 1;
            indexColumn = 1;
            CurrentPage = 0;
        }
        public void AddElement(Transform transform)
        {
            Local local = new()
            {
                page = indexPage,
                row = indexRow,
                col = indexColumn,
            };
            children.Add(local, transform);
            float x = indexRow - 1, y = indexColumn - 1;
            var origin = Template.transform.position;
            transform.position = new(origin.x + y * Spacing.x, origin.y - x * Spacing.y, 0);

            if (HorizenSort)
            {
                if (++indexColumn > MaxColumn)
                {
                    indexColumn = 1;
                    if (++indexRow > MaxRow)
                    {
                        indexRow = 1;
                        indexPage++;
                    }
                }
            }
            else
            {
                if (++indexRow > MaxRow)
                {
                    indexRow = 1;
                    if (++indexColumn > MaxColumn)
                    {
                        indexColumn = 1;
                        indexPage++;
                    }
                }
            }
        }
        public void OnPageLeftClick(GameObject go)
        {
            CurrentPage--;
            ChangePage();
            PageRight.CanBeInteract = true;
            PageRight.SetState(true);
            if (CurrentPage == 0)
            {
                PageLeft.SetState(false);
                PageLeft.CanBeInteract = false;
            }
        }
        public void OnPageRightClick(GameObject go)
        {
            CurrentPage++;
            ChangePage();
            PageLeft.CanBeInteract = true;
            PageLeft.SetState(true);
            if (CurrentPage == MaxPage - 1)
            {
                PageRight.SetState(false);
                PageRight.CanBeInteract = false;
            }
        }
        private void UpdatePageData()
        {
            MaxPage = children.Count == 0 ? 1 : children.Last().Key.page;
            PageLeft.SetState(false);
            PageLeft.CanBeInteract = false;

            bool pages = MaxPage > 1;
            PageRight.ToggleAtFirst = pages;
            PageRight.SetState(pages);
            PageRight.CanBeInteract = pages;
            ChangePage();
        }
        private void ChangePage()
        {
            StringBuilder builder = new();
            int p = CurrentPage + 1;
            builder.Append(p).Append('/').Append(MaxPage);
            string result = builder.ToString();
            PageText.Render(result);
            PageTextOutLine.Render(result);
            foreach (var (local, transform) in children)
            {
                transform.gameObject.SetActive(local.page == p);
            }
        }
        public void ReLoadPage(Action addElements)
        {
            ResetPage();
            addElements();
            UpdatePageData();
        }
        public bool TryGetChild(Predicate<GameObject> predicate, bool pageAcitve, out GameObject go)
        {
            int page = CurrentPage + 1;
            go = null;
            foreach (var (local, trans) in children)
            {
                if (pageAcitve && local.page != page)
                {
                    continue;
                }
                if (predicate.Invoke(trans.gameObject))
                {
                    go = trans.gameObject;
                    return true;
                }
            }
            return false;
        }
    }
}
