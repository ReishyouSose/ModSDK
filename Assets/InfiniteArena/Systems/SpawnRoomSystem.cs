using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.InfiniteArena.Systems
{

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class ServerSpawnRoomSystem : PugSimulationSystemBase
    {protected override void OnUpdate()
        {
            bool guestMode = WorldInfo.guestMode;
            var ecb = CreateCommandBuffer();

            base.OnUpdate();
        }
    }
}
