namespace Assets.PointShop.Scripts
{
    public class ShopSwitch : ButtonUIElement
    {
        public PugText Value;
        private void Update()
        {
            var player = Manager.main.player;
            if (player == null)
                return;
            Value.Render(player.playerInventoryHandler.GetExistingAmountOfObject(PointShop.Coin).ToString(), false, true);
        }
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            base.OnLeftClicked(mod1, mod2);
            PointShop.OpenShop();
        }
    }
}
