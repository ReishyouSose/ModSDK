using Assets.GeneralConfigMenu.Scripts;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework.Extend
{
    public class RUIScrollView : RUIElement
    {
        public Transform View;

        [Tooltip("是否允许水平滚动")]
        public bool Horizen;

        [Tooltip("是否允许垂直滚动")]
        public bool Vertical = true;

        [Tooltip("是否显示隐藏行列数")]
        public bool ShowHidden;

        [Tooltip("优先水平排布")]
        public bool HorizontalPriority = true;

        [Header("HorizenSetting")]
        public int MaxShowCol;
        public float ColSpacing;
        public RUIButton Left, Right;
        public PugText LeftCol, RightCol;

        [Header("VerticalSetting")]
        public int MaxShowRow;
        public float RowSpacing;
        public RUIButton Up, Down;
        public PugText UpRow, DownRow;
        public int TotalRow { get; private set; }
        public int TotalCol { get; private set; }
        public int ShowRow { get; private set; }
        public int ShowCol { get; private set; }
        public int MovableRow { get; private set; }
        public int MovableCol { get; private set; }

        public readonly Dictionary<RUIViewLocator, GameObject> children = new();
        private int currentRow, currentCol;
        private void OnValidate()
        {
            float per = 1 / 16f;
            RowSpacing = MathF.Round(RowSpacing / per) * per;
            ColSpacing = MathF.Round(ColSpacing / per) * per;
            MaxShowCol = Math.Max(1, MaxShowCol);
            MaxShowRow = Math.Max(1, MaxShowRow);
        }
        public void Reload(Action<RUIScrollView, Transform> added, bool clearTemplate = true, bool updateView = true)
        {
            gameObject.SetActive(false);
            Clear();
            if (clearTemplate)
            {
                foreach (Transform child in View.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            added.Invoke(this, View);
            if (updateView)
                UpdateView();
        }
        public void Clear()
        {
            currentRow = 0;
            currentCol = 0;
            TotalCol = 0;
            TotalRow = 0;
            ShowCol = 0;
            ShowRow = 0;
            MovableCol = 0;
            MovableRow = 0;
            foreach (var (_, go) in children)
            {
                Destroy(go);
            }
            children.Clear();
        }
        private void Start()
        {
            if (Horizen)
            {
                Left.AddEvent(RMouseEventType.LeftDown, _ => UpdateMovable(-1, 0));
                Right.AddEvent(RMouseEventType.LeftDown, _ => UpdateMovable(1, 0));
            }
            if (Vertical)
            {
                Up.AddEvent(RMouseEventType.LeftDown, _ => UpdateMovable(0, 1));
                Down.AddEvent(RMouseEventType.LeftDown, _ => UpdateMovable(0, -1));
            }
        }
        private void Update()
        {
            HandleMouseScroll();
        }
        private void HandleMouseScroll()
        {
            if (!IsMouseHover)
                return;
            Player rewiredPlayer = Manager.main.player.inputModule.rewiredPlayer;
            int y;
            if (rewiredPlayer.IsCurrentInputSource(92, ControllerType.Mouse) && rewiredPlayer.GetAxis(92) > 0f)
            {
                y = 1;
            }
            else if (rewiredPlayer.IsCurrentInputSource(93, ControllerType.Mouse) && rewiredPlayer.GetAxis(93) > 0f)
            {
                y = -1;
            }
            else
                return;
            bool shift = rewiredPlayer.GetButton(GeneralConfigMenuMod.HorizenScroll);
            UpdateMovable(shift ? y : 0, shift ? 0 : y);
        }
        public void AddChild(GameObject go)
        {
            var local = go.AddComponent<RUIViewLocator>();
            local.Init(this, currentRow, currentCol);
            children.Add(local, go);
            TotalCol = Math.Max(TotalCol, currentCol + 1);
            TotalRow = Math.Max(TotalRow, currentRow + 1);
            if (HorizontalPriority)
            {
                if (++currentCol >= MaxShowCol)
                {
                    currentRow++;
                    currentCol = 0;
                }
            }
            else
            {
                if (++currentRow >= MaxShowRow)
                {
                    currentCol++;
                    currentRow = 0;
                }
            }
        }
        public void UpdateView()
        {
            MovableCol = Math.Max(TotalCol - MaxShowCol, 0);
            MovableRow = Math.Max(TotalRow - MaxShowRow, 0);
            UpdateMovable(ShowCol, ShowRow);
        }

        private void UpdateMovable(int moveX, int moveY)
        {
            //int[] states = new int[5];
            int oldx = ShowCol, oldy = ShowRow;
            ShowCol = Math.Clamp(ShowCol + moveX, 0, MovableCol);
            ShowRow = Math.Clamp(ShowRow - moveY, 0, MovableRow);
            foreach (var (local, _) in children)
            {
                local.Move(oldx - ShowCol, oldy - ShowRow, out _/*var rangeState*/);
                //states[(int)rangeState]++;
            }
            if (!ShowHidden)
                return;
            if (Horizen)
            {
                LeftCol.Render(ShowCol.ToString());
                RightCol.Render((MovableCol - ShowCol).ToString());
            }
            if (Vertical)
            {
                UpRow.Render(ShowRow.ToString());
                DownRow.Render((MovableRow - ShowRow).ToString());
            }
        }
    }
}
