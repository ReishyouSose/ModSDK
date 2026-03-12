using I2.Loc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GeneralConfigMenu.RUIFramework
{
    public class RUIHoverTextContainer : MonoBehaviour
    {
        [Tooltip("Should with 75%+ opacity BG")]
        public Transform HoverTextBG;

        [Tooltip("Just create an empty")]
        public Transform HoverTopLeft;

        [Tooltip("Align Top Left")]
        public PugText HoverTextTemplate;

        private readonly List<PugText> TempTexts = new();
        internal static readonly List<RUIManager> managers = new();
        private Vector2 hoverBackgroundBounds = new Vector2(14.625f, 8.125f);

        private void Update()
        {
            UpdateHoverText();
        }

        // 原版中的UpdatePositionOfHoverText逻辑
        private Vector3 UpdatePositionOfHoverText(PugText text, Vector3 previousTextBottom, float extraSpacingFromPrevious = 0f)
        {
            float num = Math.Max(1f, 2f * text.displayedTextStringLinesAmount);
            float num2 = text.dimensions.height / num;
            float num3 = ((num2 % 0.0625f > 0f) ? (0.0625f - num2 % 0.0625f) : 0f);
            text.transform.localPosition = previousTextBottom - new Vector3(0f, num2 + num3 + extraSpacingFromPrevious, 0f);
            previousTextBottom -= new Vector3(0f, text.dimensions.height + extraSpacingFromPrevious, 0f);
            if (num3 > 0f)
            {
                previousTextBottom -= new Vector3(0f, 0.0625f, 0f);
            }

            return previousTextBottom;
        }

        private void UpdateHoverText(float maxWidthToUse = 9f)
        {
            List<TextAndFormatFields> hoverTips = new();
            foreach (var manager in managers)
            {
                hoverTips.AddRange(manager.hoverTips);
            }

            int count = hoverTips.Count;
            if (count == 0)
            {
                HoverTextBG.gameObject.SetActive(false);
                return;
            }

            HoverTextBG.gameObject.SetActive(true);

            // 初始化位置（对应原版中的vector）- 使用原版的初始值
            Vector3 currentPos = new Vector3(0.25f, -0.125f, 0f);
            float maxWidth = 0f;

            // 重置所有临时文本
            foreach (var temp in TempTexts)
            {
                temp.gameObject.SetActive(false);
            }

            // 渲染所有提示文本
            for (int i = 0; i < count; i++)
            {
                if (TempTexts.Count <= i)
                {
                    TempTexts.Add(Instantiate(HoverTextTemplate, HoverTopLeft));
                }

                var text = TempTexts[i];
                var hoverTip = hoverTips[i];

                // 设置文本属性
                text.localize = !string.IsNullOrEmpty(LocalizationManager.GetTranslation(hoverTip.text));
                text.gameObject.SetActive(true);
                text.maxWidth = maxWidthToUse;
                text.formatFields = hoverTip.formatFields;
                text.Render(hoverTip.text, false, true);

                // 设置颜色
                if (hoverTip.color != default)
                {
                    text.SetTempColor(hoverTip.color);
                }

                // 使用原版的定位逻辑，每个文本之间都有0.125f的间距
                currentPos = UpdatePositionOfHoverText(text, currentPos, 0.125f);

                // 更新最大宽度
                maxWidth = Mathf.Max(maxWidth, text.dimensions.width);
            }

            float panelWidth = maxWidth;
            float panelHeight = -currentPos.y; // currentPos.y是负值，取反得到高度

            // 超出屏幕高度，加宽后重新计算
            if (panelHeight > 16.5f && maxWidthToUse + 4f < 30f)
            {
                UpdateHoverText(maxWidthToUse + 4f);
                return;
            }

            // 设置HoverTopLeft的位置
            HoverTopLeft.localPosition = new Vector3(-panelWidth / 2f, panelHeight / 2f, 0f);

            // 背景大小计算（原版中的处理）
            panelWidth += 0.5625f;
            if (panelWidth % 0.125f != 0f)
            {
                panelWidth += 0.0625f;
            }

            panelHeight += 0.125f;
            if (panelHeight % 0.125f != 0f)
            {
                panelHeight += 0.0625f;
            }

            // 固定在右下角
            float bottomRightX = hoverBackgroundBounds.x;
            float bottomRightY = -hoverBackgroundBounds.y;

            float centerX = bottomRightX - panelWidth / 2f;
            float centerY = bottomRightY + panelHeight / 2f;

            // 对背景位置进行网格对齐
            float remainderX = centerX % 0.0625f;
            if (remainderX > 0f)
            {
                centerX += 0.0625f - remainderX;
            }

            float remainderY = centerY % 0.0625f;
            if (remainderY > 0f)
            {
                centerY += 0.0625f - remainderY;
            }

            // 应用位置和大小
            HoverTextBG.position = new Vector3(centerX, centerY, 0f);
            HoverTextBG.GetComponent<SpriteRenderer>().size = new Vector2(panelWidth, panelHeight);
        }
    }
}