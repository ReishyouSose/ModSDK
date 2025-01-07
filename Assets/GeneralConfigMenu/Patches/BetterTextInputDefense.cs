using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GeneralConfigMenu.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    public static class BetterTextInputDefense
    {
        [HarmonyPatch("HandleTypingInput")]
        [HarmonyPrefix]
        private static bool BeforeHandleTypingInput()
        {
            return true;
        }
    }
}
