using HarmonyLib;
using System.Linq;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch]
    public static class ChestSlotForceUpdatePatch
    {
        [HarmonyPatch(typeof(Chest), nameof(Chest.Use))]
        [HarmonyPostfix]
        private static void ForceUpdate()
        {
            foreach (var slot in Manager.ui.chestInventoryUI.itemSlots.Cast<InventorySlotUI>())
            {
                slot.dirty = true;
            }
        }
    }
}
