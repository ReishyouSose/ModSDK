using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class MathHelper
    {
        public static bool HasNaN(this float2 value)
        {
            return float.IsNaN(value.x) || float.IsNaN(value.y);
        }
    }
}
