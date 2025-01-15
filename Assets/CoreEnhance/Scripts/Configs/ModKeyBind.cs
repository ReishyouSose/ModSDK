using Assets.CoreEnhance.Scripts.Systems.Misc;
using Rewired;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public static class ModKeyBind
    {
        private const string CoreEnhance = "CoreEnhance:";
        internal const string QuickStack = CoreEnhance + nameof(QuickStack);
        public static void Handle(PlayerController p)
        {
            Player rewiredPlayer = p.inputModule.rewiredPlayer;
            if (rewiredPlayer.GetButtonDown(QuickStack))
            {
                QuickStackClient.SendRequest();
            }
        }
    }
}
