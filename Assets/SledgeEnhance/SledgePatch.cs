using CoreLib.Util.Extensions;
using HarmonyLib;
using System;
using UnityEngine;

namespace Assets.SledgeHammerEnhance
{
    [HarmonyPatch]
    public static class SledgePatch
    {
        [HarmonyPatch(typeof(WeaponConverter), nameof(WeaponConverter.Convert))]
        [HarmonyPrefix]
        private static void WeaponPatch(WeaponAuthoring authoring)
        {
            if (authoring.gameObject.GetEntityObjectID().ToString().EndsWith("Sledge"))
            {
                float range = Math.Max(global::SledgeEnhance.range.Value, 1.4f);
                Debug.Log("Sledge range: " + range);
                authoring.baseHitColliderSize = range;
            }
        }

        [HarmonyPatch(typeof(CooldownConverter), nameof(CooldownConverter.Convert))]
        [HarmonyPrefix]
        private static void CDPatch(CooldownAuthoring authoring)
        {
            if (authoring.gameObject.GetEntityObjectID().ToString().EndsWith("Sledge"))
            {
                float speed = global::SledgeEnhance.speed.Value;
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
