using Assets.CoreEnhance.Scripts.Systems.Misc;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class VerdantShrineTerminalClient : PugSimulationSystemBase
    {
        public static void OpenVSTerminal()
        {
            //ins.queue.Enqueue(new(Manager.main.player.entity));
        }
    }

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class VerdantShrineTerminalServer: PugSimulationSystemBase
    {

    }
}
