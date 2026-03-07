using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using PugMod;
using Rewired;
using UnityEngine;

namespace Assets.PointShop
{
    public class PointShop : IMod
    {
        private const string UIName = "PointShop_PointShopMenu";
        private const string Open = "PointShop_Open";
        public void EarlyInit()
        {
            CoreLibMod.LoadSubmodule(typeof(UserInterfaceModule), typeof(ControlMappingModule));
            ControlMappingModule.AddKeyboardBind(Open, KeyboardKeyCode.O, ModifierKey.Control);
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is GameObject gameObj)
            {
                UserInterfaceModule.RegisterModUI(gameObj);
            }
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            var player = Manager.main.player;
            if (player == null)
                return;
            if (player.inputModule.rewiredPlayer.GetButtonDown(Open))
                UserInterfaceModule.OpenModUI(UIName);
        }
    }
}
