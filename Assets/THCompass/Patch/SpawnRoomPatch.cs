using Assets.THCompass.Helper;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.THCompass.Patch
{
    /*[HarmonyPatch]
    public static class SpawnRoomPatch
    {
        internal static List<int> AvailableScenes { get; private set; }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnCustomSceneSystem), "OnStartRunning")]
        private static void GetWhenStart(SpawnCustomSceneSystem __instance)
        {
            AvailableScenes = __instance.GetField<List<int>>("availableSceneIndices");
            Debug.Log("Get List When Start");
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnCustomSceneSystem), "TrySpawnRandomCustomScene")]
        private static void GetWhenTrySpawn(SpawnCustomSceneSystem __instance, EntityCommandBuffer ecb,
            SpawnedArea area, CustomScenesDataTable customScenesData, uint seed,
            NativeArray<BlockedSpawnAreaCD> blockedAreas, NativeArray<BiomeRanges> biomeRanges,
            DisableEntitiesSystem disableEntitiesSystem)
        {
            AvailableScenes = __instance.GetField<List<int>>("availableSceneIndices");
            Debug.Log("Get List When TrySpawn");
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnCustomSceneSystem), "OnUpdate")]
        private static void GetWhenUpdate(SpawnCustomSceneSystem __instance)
        {
            if (AvailableScenes == null)
            {
                AvailableScenes = __instance.GetField<List<int>>("availableSceneIndices");
                Debug.Log("Get List When Update");
            }
        }
    }*/
}
