using HarmonyLib;
using Interaction;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch(typeof(InteractablePostConverter))]
    public static class InteractablePatch
    {
        [HarmonyPatch(nameof(InteractablePostConverter.PostConvert))]
        [HarmonyPrefix]
        private static bool Validata(GameObject authoring)
        {
            if (authoring.TryGetComponent<EntityMonoBehaviourData>(out var _))
                return true;
            if (authoring.TryGetComponent<ObjectAuthoring>(out var info))
                Debug.Log(info.objectName);
            return true;
        }
    }
}
