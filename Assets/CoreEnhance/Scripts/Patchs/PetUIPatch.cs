using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using HarmonyLib;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using UnityEngine;
using static PugDatabase;

namespace Assets.CoreEnhance.Scripts.Patchs
{
    [HarmonyPatch]
    public static class PetUIPatch
    {
        [HarmonyPatch(typeof(PetTalentsWindow), nameof(PetTalentsWindow.Awake)), HarmonyPostfix]
        private static void AddResetSkinFunc(PetTalentsWindow __instance)
        {
            __instance.resetButton.onRightClick.AddListener(new(ModifyPet));
        }

        [HarmonyPatch(typeof(PetTalentUIElement), nameof(PetTalentUIElement.OnLeftClicked)), HarmonyPrefix]
        private static bool RollSingleSkill(int ____talentIndex, bool mod1, bool mod2)
        {
            PlayerController player = Manager.main.player;
            if (player == null)
                return false;
            bool fix = mod1 || mod2 || Input.GetKey(KeyCode.LeftAlt);
            if (!fix)
                return true;
            PetModifierClient.ModifyPet(PetModifyID.RollSingleSkill, player.entity, ____talentIndex);
            return false;
        }

        [HarmonyPatch(typeof(PetTalentsWindow), "UpdateTalents"), HarmonyPostfix]
        private static void AllowClick(PetTalentsWindow __instance)
        {
            __instance.resetButton.canBeClicked = true;
        }

        [HarmonyPatch(typeof(PetTalentsWindow), "CanResetTalents"), HarmonyPrefix]
        private static bool CanResetTalents(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SlotUIBase), nameof(SlotUIBase.GetHoverStats),
            new Type[] { typeof(ContainedObjectsBuffer), typeof(bool), typeof(bool) })]
        private static void HoverStats(ContainedObjectsBuffer containedObject,
            List<TextAndFormatFields> __result)
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.TalentDisplay))
                return;
            var player = Manager.main.player;
            var world = player.world;
            DatabaseBankCD singleton = player.querySystem.GetSingleton<DatabaseBankCD>();
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
        private static void ModifyPet()
        {
            PlayerController player = Manager.main.player;
            if (player == null)
                return;
            bool fix = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl);
            PetModifierClient.ModifyPet(fix ? PetModifyID.RollAllSkill : PetModifyID.ResetSkin, player.entity);
        }
    }
}
