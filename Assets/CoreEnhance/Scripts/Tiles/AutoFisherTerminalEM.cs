using Assets.CoreEnhance.Scripts.Systems.Automation;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherTerminalEM : ContainerEM
    {
        public void OnOpenTerminal() => AutoFisherTerminalClient.OpenTerminal();
    }
}
