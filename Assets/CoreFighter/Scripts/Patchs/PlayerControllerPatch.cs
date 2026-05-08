using Assets.CoreFighter.Scripts.Systems.Equip;
using HarmonyLib;
using PlayerEquipment;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Patchs
{
    [HarmonyPatch(typeof(PlayerController))]
    public static class PlayerControllerPatch
    {
        private static readonly int EmissiveTex = Shader.PropertyToID("_EmissiveTex");

        [HarmonyPatch(nameof(PlayerController.UpdateEquippedSlotVisuals)), HarmonyPostfix]
        private static void UpdateEquippedSlotVisualsPrefix(PlayerController __instance, EquipmentSlotType ___visuallyEquippedSlotType)
        {
            var contained = __instance.visuallyEquippedContainedObject;
            ObjectID id = contained.objectID;
            ObjectInfo objectInfo2 = PugDatabase.GetObjectInfo(id, contained.variation);
            // 只在需要的时候修正
            switch (___visuallyEquippedSlotType)
            {
                case EquipmentSlotType.MeleeWeaponSlot:
                case EquipmentSlotType.ShovelSlot:
                case EquipmentSlotType.HoeSlot:
                case EquipmentSlotType.BugNet:
                case EquipmentSlotType.SeederSlot:
                    if (PugDatabase.TryGetComponent<MeleeWeaponCD>(id, out var melee))
                    {
                        if (melee.isBigSpearWeapon || melee.isBigSwingWeapon)
                            return;
                        HideAllEquippedSlotVisuals(__instance);
                        if (PugDatabase.HasComponent<NativeMoveFreelyCD>(id, 0))
                        {
                            ActivateCarryableItemSpriteAndSkin(__instance, __instance.carryableDrillToolSprite, __instance.carryableDrillToolSkin, objectInfo2, __instance.visuallyEquippedContainedObject);
                        }
                        else
                        {
                            ActivateCarryableItemSpriteAndSkin(__instance, __instance.carryableSwingItemSprite, __instance.carryableSwingItemSkinSkin, objectInfo2, __instance.visuallyEquippedContainedObject);
                        }
                    }
                    break;
                case EquipmentSlotType.BeamWeaponSlot:
                    HideAllEquippedSlotVisuals(__instance);
                    if (PugDatabase.HasComponent<NativeMoveFreelyCD>(id, 0))
                    {
                        if (PugDatabase.TryGetComponent(id, out BeamWeaponCD beamWeaponCD) && beamWeaponCD.useRangedLoopAnimation)
                        {
                            ActivateCarryableItemSpriteAndSkin(__instance, __instance.carryableRangeItemSprite, __instance.carryableRangeItemSkinSkin, objectInfo2, __instance.visuallyEquippedContainedObject);
                        }
                        else
                        {
                            ActivateCarryableItemSpriteAndSkin(__instance, __instance.carryableDrillToolSprite, __instance.carryableDrillToolSkin, objectInfo2, __instance.visuallyEquippedContainedObject);
                        }
                    }
                    else
                    {
                        ActivateCarryableItemSpriteAndSkin(__instance, __instance.carryableSwingItemSprite, __instance.carryableSwingItemSkinSkin, objectInfo2, __instance.visuallyEquippedContainedObject);
                    }
                    break;
                default:
                    return;
            }
            foreach (var carryableEffect in __instance.carryableEffects)
            {
                if (carryableEffect.objectWithEffect == id)
                {
                    carryableEffect.effectObject.SetActive(true);
                }
                else
                {
                    carryableEffect.effectObject.SetActive(false);
                }
            }
            __instance.animator.Update(0f);
        }
        private static void ActivateCarryableItemSpriteAndSkin(PlayerController pc, SpriteRenderer carryableSprite, SpriteSheetSkin skin, ObjectInfo entityInfo, ContainedObjectsBuffer visuallyEquipped)
        {
            carryableSprite.gameObject.SetActive(true);
            Sprite iconOverride = Manager.ui.itemOverridesTable.GetIconOverride(pc.objectData, false);
            Sprite sprite = ((iconOverride != null) ? iconOverride : ((entityInfo != null) ? ((entityInfo.additionalSprites.Count > 0) ? entityInfo.additionalSprites[0] : entityInfo.icon) : null));
            carryableSprite.sprite = sprite;
            skin.SetSkin((carryableSprite.sprite != null) ? carryableSprite.sprite.texture : null);
            carryableSprite.material.SetTexture(EmissiveTex, (entityInfo != null && entityInfo.additionalSprites.Count > 1) ? entityInfo.additionalSprites[1].texture : null);
            Manager.ui.ApplyAnyIconGradientMap(visuallyEquipped, carryableSprite);
        }
        private static void HideAllEquippedSlotVisuals(PlayerController pc)
        {
            pc.carryableSwingItemSprite.gameObject.SetActive(false);
            pc.carryableRangeItemSprite.gameObject.SetActive(false);
            pc.carryableShieldItemSprite.gameObject.SetActive(false);
            pc.carryableBigSpearItemSprite.gameObject.SetActive(false);
            pc.carryableBigSwingItemSprite.gameObject.SetActive(false);
            pc.carryableDrillToolSprite.gameObject.SetActive(false);
            pc.carryableTorch.SetActive(false);
            pc.carryablePlaceItemSprite.gameObject.SetActive(false);
            pc.carryableFishingRodSprite.gameObject.SetActive(false);
            pc.instrumentSprite.gameObject.SetActive(false);
        }
    }
}
