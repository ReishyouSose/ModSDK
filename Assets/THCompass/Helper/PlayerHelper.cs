using PugMod;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Assets.THCompass.Helper
{
    public static class PlayerHelper
    {
        public static void ConsumeItem(this PlayerController player, int amount)
        {
            EquipmentSlot slot = player.GetEquippedSlot();
            int index = slot.inventoryIndexReference;
            player.playerInventoryHandler.Consume(index, amount, false);
            player.SetEquipmentSlotToNonUsableIfEmptySlot(slot);
        }

        public static Entity GetPlayerEntity(this Entity sender)
        {
            EntityManager entityManager = API.Server.World.EntityManager;
            Entity playerEntity = entityManager.GetComponentData<CommandTarget>(sender).targetEntity;
            return playerEntity;
        }
        public static float3 WorldTilePos(this PlayerController p) => p.WorldPosition.RountToFloat3();
        public static int2 RenderTilePos(this PlayerController p) => EntityMonoBehaviour.ToRenderFromWorld(p.WorldPosition.RoundToInt2());
    }
}
