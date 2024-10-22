using HarmonyLib;
using PlayerState;
using PugMod;
using Unity.Entities;

namespace Assets.ImproveEquipment.Patchs
{
    [HarmonyPatch]
    public static class PushBackPatch
    {
        [HarmonyPatch(typeof(ReceivedPushbackCD), nameof(ReceivedPushbackCD.TryAddPushback))]
        [HarmonyPrefix]
        private static bool RemovePush(Entity targetEntity)
        {
            var manager = API.Server.World.EntityManager;
            if (manager.HasComponent<PlayerStateCD>(targetEntity))
            {
                return false;
            }
            return true;
        }
    }
}
