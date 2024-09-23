using CoreLib.Util.Extensions;
using HarmonyLib;
using System;
using UnityEngine;

namespace Assets.ImproveEquipment.Patchs
{
    [HarmonyPatch]
    public static class SledgePatch
    {
        private static ModConfig Config => ImproveEquipmentMod.config;
        [HarmonyPatch(typeof(WeaponConverter), nameof(WeaponConverter.Convert))]
        [HarmonyPrefix]
        private static void WeaponPatch(WeaponAuthoring authoring)
        {
            if (Config.Sledge_Enable.Value == false)
                return;
            if (authoring.gameObject.GetEntityObjectID().ToString().EndsWith("Sledge"))
            {
                float range = Math.Max(Config.Sledge_Range.Value, 1.4f);
                Debug.Log("Sledge range: " + range);
                authoring.baseHitColliderSize = range;
            }
        }

        [HarmonyPatch(typeof(CooldownConverter), nameof(CooldownConverter.Convert))]
        [HarmonyPrefix]
        private static void CDPatch(CooldownAuthoring authoring)
        {
            if (Config.Sledge_Enable.Value == false)
                return;
            if (authoring.gameObject.GetEntityObjectID().ToString().EndsWith("Sledge"))
            {
                float speed = Config.Sledge_Speed.Value;
                if (speed <= 0)
                {
                    speed = 0.7f;
                }
                else
                    speed = Math.Min(speed, 0.7f);
                Debug.Log("Sledge speed: " + 1 / speed + "/s");
                authoring.cooldown = speed;
            }
        }
    }
}
