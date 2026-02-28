using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CoreFighter.Scripts.Patchs
{
    [HarmonyPatch]
    public static class CDDisplayPatch
    {
        private const string cooldownSecString = "CooldownSec";
        private const string cooldownMinString = "CooldownMin";
        private const string cooldownMinSecString = "CooldownMinSec";

        [HarmonyPatch(typeof(SlotUIBase), nameof(SlotUIBase.GetHoverStats),
            new Type[] { typeof(ContainedObjectsBuffer), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        private static void AddLesserThan1CDState(ContainedObjectsBuffer containedObject,
            List<TextAndFormatFields> __result)
        {
            if (__result == null)
                return;
            if (!PugDatabase.HasComponent<HasWeaponDamageCD>(containedObject.objectData) &&
                PugDatabase.TryGetComponent<CooldownCD>(containedObject.objectData, out var cooldownCD))
            {
                float cooldown = cooldownCD.cooldown;
                if (cooldown < 1f)
                {
                    if (cooldown == 0f)
                        return;
                    ConditionUI.DurationFormat durationStrings = ConditionUI.GetDurationStrings(cooldown, out string text9, out string text10, true);
                    string[] formatFields = Array.Empty<string>();
                    string text11 = cooldownSecString;
                    switch (durationStrings)
                    {
                        case ConditionUI.DurationFormat.SEC:
                            formatFields = new string[]
                            {
                                text9
                            };
                            text11 = cooldownSecString;
                            break;
                        case ConditionUI.DurationFormat.MIN:
                            formatFields = new string[]
                            {
                                text10
                            };
                            text11 = cooldownMinString;
                            break;
                        case ConditionUI.DurationFormat.MIN_AND_SEC:
                            formatFields = new string[]
                            {
                                text10,
                                text9
                            };
                            text11 = cooldownMinSecString;
                            break;
                    }
                    __result.Add(new TextAndFormatFields
                    {
                        text = text11,
                        formatFields = formatFields
                    });
                }
            }
        }
    }
}
