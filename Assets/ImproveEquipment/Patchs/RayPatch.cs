using CoreLib.Util.Extensions;
using HarmonyLib;

namespace Assets.ImproveEquipment.Patchs
{
    [HarmonyPatch]
    public static class RayPatch
    {
        private static ModConfig Config => ImproveEquipmentMod.config;
        [HarmonyPatch(typeof(WeaponConverter), nameof(WeaponConverter.Convert))]
        [HarmonyPrefix]
        private static void WeaponModifier(WeaponAuthoring authoring)
        {
            if (!Config.Ray_Enable.Value)
                return;
            ObjectID id = authoring.GetEntityObjectID();
            float mult = Config.Ray_MoveSpeed.Value;
            if (mult <= 0f)
            {
                mult = 1f;
            }
            else if (mult > 2f)
                mult = 2f;
            if (id is ObjectID.LaserDrillTool or ObjectID.LightningGun)
            {
                authoring.moveSpeedMultiplier = mult;
            }
        }
    }
}
