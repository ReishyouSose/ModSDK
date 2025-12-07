using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using HarmonyLib;
using I2.Loc;
using Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [HarmonyPatch(typeof(SlotUIBase), nameof(SlotUIBase.GetHoverStats),
            new Type[] { typeof(ContainedObjectsBuffer), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        private static void HoverStats(ContainedObjectsBuffer containedObject,
            List<TextAndFormatFields> __result)
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.Misc, EC_Misc.PetTalentDisplay))
                return;
            var player = Manager.main.player;
            var world = player.world;
            DatabaseBankCD singleton = world.EntityManager.GetDatabaseBankCD();
            Entity entity = GetPrimaryPrefabEntity(containedObject.objectID,
                singleton.databaseBankBlob, containedObject.variation);
            if (player.activePet != null && entity == player.activePet.entity)
                return;
            if (!EntityUtility.TryGetComponentData(entity, world, out PetCD pet))
                return;
            string petType = pet.petType.ToString();
            if (!InventoryHandler.TryGetExtraInventoryBuffer<PetTalentBuffer>(containedObject, out var buffers))
                return;
            __result ??= new();
            Dictionary<PetTalent, int> talents = new();
            for (int i = 0; i < buffers.Length; i++)
            {
                var id = buffers[i].petTalentID;
                talents.TryGetValue(id, out int count);
                talents[id] = count + 1;
            }
            int j = 0;
            StringBuilder builder = new();
            foreach (var talent in talents.OrderByDescending(kv => kv.Value))
            {
                builder.Append(' ')
                    .Append(LocalizationManager.GetTranslation("PetTalents/" + talent.Key.ToString() + petType))
                    .Append("*")
                    .Append(talent.Value);
            }
            __result.Add(new()
            {
                text = "points",
                formatFields = new string[] { builder.ToString() },
                dontLocalizeFormatFields = true,
            });
        }

        private static bool HasEnoughCoin(PlayerController player)
        {
            BufferLookup<ContainedObjectsBuffer> container = player.querySystem.GetBufferLookup<ContainedObjectsBuffer>(true);
            BufferLookup<InventoryBuffer> inventory = player.querySystem.GetBufferLookup<InventoryBuffer>(true);
            DatabaseBankCD singleton = Manager.ecs.ClientWorld.EntityManager.GetDatabaseBankCD();
            //player.querySystem.GetSingleton<DatabaseBankCD>();
            return InventoryUtility.GetTotalAmount(container, inventory, singleton, player.entity, ObjectID.AncientCoin) >= 200;
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
