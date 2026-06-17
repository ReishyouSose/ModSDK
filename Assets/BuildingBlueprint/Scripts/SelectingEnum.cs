using System;

namespace Assets.BuildingBlueprint.Scripts
{
    /// <summary>
    /// 选区绘制模式
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>框选模式（拖拽矩形框）</summary>
        Box,
        /// <summary>点选模式（点击选取）</summary>
        Click,
        /// <summary>信息检查模式</summary>
        Check,
    }

    /// <summary>
    /// 选区运算(用于<see cref="SelectionMode.Box"/>模式）
    /// </summary>
    public enum BoxOperator
    {
        New,       // 另起
        Add,       // 添加
        Subtract,  // 排除重叠部分
        Intersect  // 选中重叠部分
    }

    /// <summary>
    /// 点选运算（用于<see cref="SelectionMode.Point"/>模式）
    /// </summary>
    public enum ClickOperator
    {
        Add,
        Remove
    }

    public enum SelectionLayer
    {
        Entity,
        Tile,
    }
}
