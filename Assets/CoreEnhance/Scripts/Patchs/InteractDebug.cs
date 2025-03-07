using HarmonyLib;
using Interaction;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch]
    public static class InteractDebug
    {
        [HarmonyPatch(typeof(InteractablePostConverter), nameof(InteractablePostConverter.PostConvert))]
        [HarmonyPrefix]
        private static bool TryDebug(GameObject authoring)
        {
            return authoring.TryGetComponent<EntityMonoBehaviourData>(out _);
        }
    }
}
