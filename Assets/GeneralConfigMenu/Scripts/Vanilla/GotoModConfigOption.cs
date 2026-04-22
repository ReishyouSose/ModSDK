namespace Assets.GeneralConfigMenu.Scripts.Vanilla
{
    public class GotoModConfigOption : RadicalMenuOption
    {
        public override void OnActivated()
        {
            base.OnActivated();
            Manager.menu.PushMenu(GeneralConfigMenuMod.Menu);
        }
    }
}
