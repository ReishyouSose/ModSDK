using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework.Extend
{
    public class RUIViewLocator : MonoBehaviour
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int OriRow { get; private set; }
        public int OriCol { get; private set; }
        public RUIScrollView View { get; private set; }
        private bool init;
        private Vector3 origin;
        public void Init(RUIScrollView view, int row, int col)
        {
            if (init)
                return;
            init = true;
            View = view;
            OriRow = Row = row;
            OriCol = Col = col;
            origin = transform.localPosition;
        }
        public bool InShowRange(out RangeState outRange)
        {
            int showRow = View.ShowRow, showCol = View.ShowCol;
            if (OriRow < showRow)
            {
                outRange = RangeState.Top;
                return false;
            }
            if (OriRow >= showRow + View.MaxShowRow)
            {
                outRange = RangeState.Bottom;
                return false;
            }
            if (OriCol < showCol)
            {
                outRange = RangeState.Left;
                return false;
            }
            if (OriCol >= showCol + View.MaxShowCol)
            {
                outRange = RangeState.Right;
                return false;
            }
            outRange = RangeState.InRange;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveX"></param>
        /// <param name="moveY"></param>
        /// <param name="bufferInShowCol"></param>
        /// <param name="bufferInShowRow"></param>
        /// <returns>InRange</returns>
        public void Move(int moveX, int moveY, out RangeState rangeState)
        {
            Col += moveX;
            Row += moveY;
            bool inrange = InShowRange(out rangeState);
            gameObject.SetActive(inrange);
            transform.localPosition = origin + new Vector3(View.ColSpacing * Col, -View.RowSpacing * Row, 0);
        }
    }
    public enum RangeState
    {
        InRange,
        Left,
        Right,
        Top,
        Bottom,
    }
}
