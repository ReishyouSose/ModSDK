using Unity.Entities;

namespace Assets.PointShop.Scripts
{
    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class BossDefeatedSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            if (!SystemAPI.HasSingleton<BossDefeatedInfo>())
            {
                EntityManager.CreateEntity(typeof(BossDefeatedInfo));
            }
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<BossDefeatedInfo>(out var info))
                return;
            if (!SystemAPI.TryGetSingletonBuffer<KilledEnemiesBuffer>(out var buffer))
                return;
            foreach (var killed in buffer)
            {
                switch (killed.objectData.objectID)
                {
                    case ObjectID.SlimeBoss:
                        info.Slime = true;
                        break;
                    case ObjectID.BossLarva:
                        info.Devourer = true;
                        break;
                    case ObjectID.LarvaHiveBoss:
                        info.LarvaHive = true;
                        break;
                    case ObjectID.ShamanBoss:
                        info.Shaman = true;
                        break;
                    case ObjectID.BirdBoss:
                        info.Bird = true;
                        break;
                    case ObjectID.PoisonSlime:
                        info.PoisonSlime = true;
                        break;
                    case ObjectID.OctopusBoss:
                        info.Octopus = true;
                        break;
                    case ObjectID.ScarabBoss:
                        info.Scarab = true;
                        break;
                    case ObjectID.LavaSlimeBoss:
                        info.LavaSlime = true;
                        break;
                    case ObjectID.HydraBossNature:
                        info.HydraNature = true;
                        break;
                    case ObjectID.HydraBossSea:
                        info.HydraSea = true;
                        break;
                    case ObjectID.HydraBossDesert:
                        info.HydraDesert = true;
                        break;
                    case ObjectID.SnakeBossSegment:
                        info.Atlantian = true;
                        break;
                    case ObjectID.CoreBoss:
                        info.CoreCommander = true;
                        break;
                    case ObjectID.WallBoss:
                        info.WallSlime = true;
                        break;
                    case ObjectID.GiantCicadaBoss:
                        info.Cicada = true;
                        break;
                    case ObjectID.RobotBoss:
                        info.Robot = true;
                        break;
                }
            }
            SystemAPI.SetSingleton(info);
            base.OnUpdate();
        }
    }
}
