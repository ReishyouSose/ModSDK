using Assets.CoreEnhance.Scripts.Systems.Misc;
using HarmonyLib;
using Inventory;
using Unity.Entities;
using static PugDatabase;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch]
    public static class PetUIPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PetTalentsWindow), nameof(PetTalentsWindow.Awake))]
        private static void AddResetSkinFunc(PetTalentsWindow __instance)
        {
            __instance.resetButton.onRightClick.AddListener(new(ResetSkin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PetTalentsWindow), "UpdateTalents")]
        private static void AllowClick(PetTalentsWindow __instance)
        {
            __instance.resetButton.canBeClicked = true;
        }

        private static bool HasEnoughCoin(PlayerController player)
        {
            BufferLookup<ContainedObjectsBuffer> bufferLookup = player.querySystem.GetBufferLookup<ContainedObjectsBuffer>(true);
            DatabaseBankCD singleton = player.querySystem.GetSingleton<DatabaseBankCD>();
            return InventoryUtility.GetTotalAmount(bufferLookup, singleton, player.entity, ObjectID.AncientCoin) >= 200;
        }

        private static void ResetSkin()
        {
            PlayerController player = Manager.main.player;
            if (player == null)
                return;
            if (HasEnoughCoin(player))
                PetModifierClient.ResetSkin(player.entity);
        }
    }
}
