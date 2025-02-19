using Assets.CoreEnhance.Scripts.Systems.Automation;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class VerdantShrineTerminalEM : ContainerEM
    {
        public void OnOpenTerminal() => VerdantShrineTerminalClient.OpenTerminal();
    }
}
