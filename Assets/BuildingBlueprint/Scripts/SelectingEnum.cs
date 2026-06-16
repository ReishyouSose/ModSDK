using System;

namespace Assets.BuildingBlueprint.Scripts
{
    /// <summary>
    /// 选区绘制模式
    /// </summary>
    public enum SelectionMode
    {
        None,
        /// <summary>框选模式（拖拽矩形框）</summary>
        Box,
        /// <summary>点选模式（点击选取）</summary>
        Click
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

    [Flags]
    public enum SelectTarget : uint
    {
        None = 1 << 0,
        Entity = 1 << 1,
        Tile = 1 << 2,
        Ground = 1 << 3,
    }
}
