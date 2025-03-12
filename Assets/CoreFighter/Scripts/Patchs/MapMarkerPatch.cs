using Assets.CoreFighter.Scripts.Configs;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Patchs
{
    [HarmonyPatch(typeof(MapMarkerUIElement))]
    public static class MapMarkerPatch
    {
        private static HashSet<MapMarkerType> allow = new()
        {
            MapMarkerType.PlayerGrave,
            MapMarkerType.Portal,
            MapMarkerType.Waypoint,
            MapMarkerType.UserPlacedMarker
        };
        [HarmonyPatch(nameof(MapMarkerUIElement.GetHoverDescription))]
        [HarmonyPrefix]
        private static bool OverrideHoverText(MapMarkerUIElement __instance, ref List<TextAndFormatFields> __result)
        {
            if (!FighterConfig.IsEnable(FighterCategory.Misc, FC_Misc.MapMarkerTeleport))
                return true;
            if (__instance.mapMarkerEntity == Entity.Null)
            {
                return false;
            }
            PlayerController player = Manager.main.player;
            if (player == null)
                return false;
            var markType = __instance.markerType;
            if (allow.Contains(markType))
            {
                __result = new();
                bool press = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                Color c = press ? (Color.white * 0.99f) : Manager.ui.brokenColor;
                __result.Add(new()
                {
                    text = "CoreFighter/PressShift",
                    color = c,
                });
                __result.Add(new()
                {
                    text = "MapMarkers/portalGoToDesc",
                    color = c,
                });
            }
            return false;
        }

        [HarmonyPatch(nameof(MapMarkerUIElement.OnLeftClicked))]
        [HarmonyPrefix]
        private static bool OverrideLeftClicked(MapMarkerUIElement __instance, bool mod1, bool mod2)
        {
            if (!FighterConfig.IsEnable(FighterCategory.Misc, FC_Misc.MapMarkerTeleport))
                return true;
            if (!mod2)
                return false;
            var entity = __instance.mapMarkerEntity;
            if (entity == Entity.Null)
                return false;
            var markType = __instance.markerType;
            if (!allow.Contains(markType))
                return false;
            var world = __instance.world;
            PlayerController player = Manager.main.player;
            float2 rhs = EntityUtility.GetObjectData(entity, world).variation == 20 ? new(1f, 1f) : new(1f, -0.25f);
            player.QueueInputAction(new()
            {
                action = UIInputAction.Teleport,
                position = EntityUtility.GetComponentData<LocalTransform>(entity, world).Position.ToFloat2() + rhs
            });
            if (Manager.ui.isShowingMap)
            {
                Manager.ui.OnMapToggle();
            }
            return false;
        }
    }
}
