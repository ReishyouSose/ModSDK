using Unity.Mathematics;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class MathHelper
    {
        public static bool HasNaN(this float2 value)
        {
            return float.IsNaN(value.x) || float.IsNaN(value.y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxDecimal">保留小数位</param>
        /// <returns></returns>
        public static string ToPercent(this float number, int maxDecimal = 2)
        {
            string input = number.ToString();
            // 如果输入是空或 null，直接返回
            if (string.IsNullOrEmpty(input))
                return input;

            // 如果输入没有小数点，直接补全为 ".00"
            if (!input.Contains("."))
                return input + "00%";

            // 分离整数部分和小数部分
            string[] parts = input.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts[1];

            // 处理整数部分：如果为空（如 ".67"），补全为 "0"
            if (string.IsNullOrEmpty(integerPart))
                integerPart = "0";

            // 处理小数部分：去掉末尾的 0，最多保留 4 位小数
            decimalPart = decimalPart.TrimEnd('0');
            if (decimalPart.Length > maxDecimal)
                decimalPart = decimalPart[..maxDecimal];

            if (string.IsNullOrEmpty(decimalPart))
            {
                return integerPart + "%";
            }

            return $"{integerPart}.{decimalPart}%";
        }
    }
}
