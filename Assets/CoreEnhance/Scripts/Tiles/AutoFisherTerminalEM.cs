using Assets.CoreEnhance.Scripts.Systems.Misc;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherTerminalEM : Chest
    {
        public override void Use()
        {
            base.Use();
            AutoFisherTerminalClient.OpenAFTerminal();
        }
    }
}
