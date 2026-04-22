using HarmonyLib;
using System;
using Object = UnityEngine.Object;

namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    [HarmonyPatch]
    public static class MenuPatch
    {
        private const RadicalMenu.MenuType Menu = (RadicalMenu.MenuType)1493;
        private static RadicalMenu MenuInstance;
        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Init)), HarmonyPrefix]
        public static void MenuManager_PreInit(MenuManager __instance)
        {
            var menu = __instance.optionsMenuPrefab;
            var options = menu.GetComponentsInChildren<RadicalOptionsMenuOption_PushMenu>();
            var option = Array.Find(options, x => x is RadicalOptionsMenuOption_PushMenu push && push.menuToPush == RadicalMenu.MenuType.UI_OPTIONS).transform;
            int index = option.GetSiblingIndex();
            var modConfig = Object.Instantiate(option);
            modConfig.SetParent(option.parent);
            modConfig.SetSiblingIndex(index + 1);
            modConfig.name = nameof(GotoModConfigOption);
            var obj = modConfig.gameObject;
            obj.GetComponentInChildren<PugText>().SetText("GeneralConfigMenu/ModConfig");
            var config = obj.GetComponent<RadicalOptionsMenuOption_PushMenu>();
            config.menuToPush = GeneralConfigMenuMod.Menu;
        }

        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Init)), HarmonyPostfix]
        public static void MenuManager_PostInit()
        {
            var menu = Object.Instantiate(GeneralConfigMenuMod.MenuPrefab, Manager.camera.uiCamera.transform).GetComponent<ModConfigMenu>();
            menu.gameObject.SetActive(false);
            MenuInstance = menu;
        }

        [HarmonyPatch(typeof(RadicalMenu), nameof(RadicalMenu.TypeToMenu)), HarmonyPrefix]
        public static bool RadicalMenu_TypeToMenu(RadicalMenu.MenuType type, ref RadicalMenu __result)
        {
            if (type == GeneralConfigMenuMod.Menu)
            {
                __result = MenuInstance;
                return false;
            }
            return true;
        }
    }
}
