using Unity.Entities;
using UnityEngine;

namespace Assets.SoulLinkKeeper
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation, WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class DeathReportSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            Entities.ForEach((ref DeathSpreadCD death, in PlayerCustomizationCD custom) =>
            {
                ref var client = ref death.Client;
                int server = death.Server;
                if (client > server)
                    client = server;
                if (client == server)
                    return;
                client = server;
                Debug.Log("report spread");
                Manager.ui.chatWindow.AddInfoText(new string[2] { custom.customization.name.ToString(), server.ToString() }, SoulLinkKeeper.PlayerDeathType);
            })
                .WithName("ReportDeathSpread")
                .WithoutBurst()
                .Run();
            base.OnUpdate();
        }
    }
}
