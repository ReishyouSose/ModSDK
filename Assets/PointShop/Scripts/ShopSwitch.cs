using Assets.GeneralConfigMenu.RUIFramework;

namespace Assets.PointShop.Scripts
{
    public class ShopSwitch : RUIButton
    {
        public PugText Value;
        private void Update()
        {
            var player = Manager.main.player;
            if (player == null)
                return;
            Value.Render(player.playerInventoryHandler.GetExistingAmountOfObject(PointShop.Coin).ToString(), false, true);
        }
        protected override void Awake()
        {
            base.Awake();
            AddEvent(RMouseEventType.LeftClick, _ => PointShop.OpenShop());
        }
    }
}
