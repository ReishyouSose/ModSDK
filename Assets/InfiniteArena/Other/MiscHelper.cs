using System.Linq;
using Unity.Mathematics;

namespace Assets.InfiniteArena.Other
{
    public static class MiscHelper
    {
        public const string ModName = "InfiniteArena/";
        public static void CombatInfo(string key, float3 pos, params object[] args)
        {
            CombatText.SpawnCombatText(ModName + key, CombatText.NumberColor.White,
                pos - Manager.camera.RenderOrigo.ToFloat3(), false, false, true,
                args.Select(x => x.ToString()).ToArray(), false);
        }
    }
}
