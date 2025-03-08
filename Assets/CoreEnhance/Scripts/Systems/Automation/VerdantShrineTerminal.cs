using Assets.CoreEnhance.Scripts.Buffers;
using Assets.CoreEnhance.Scripts.Components;
using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using PugProperties;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class VerdantShrineTerminalClient : PugSimulationSystemBase
    {
        private static VerdantShrineTerminalClient ins;
        private NativeQueue<OpenTerminalCD> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(OpenTerminalCD), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out _))
            {
                ecb.CreateEntity(archetype);
            }
            base.OnUpdate();
        }
        public static void OpenTerminal()
        {
            var skill = SkillID.Gardening;
            int skillValue = Manager.saves.GetSkillValue(skill);
            int num = (int)math.floor(SkillExtensions.GetLevelFromSkill(skill, skillValue) / 5f);
            if (num >= 100)
                return;
            ins.queue.Enqueue(new(Manager.main.player.entity, TerminalType.VerdantShrine));
        }
    }

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateAfter(typeof(VerdantShrineSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class VerdantShrineTerminalServer : PugSimulationSystemBase
    {
        private ComponentLookup<ObjectPropertiesCD> propertiesLookup;
        private ComponentLookup<ObjectDataCD> objLookup;
        private ComponentLookup<PlantCD> plantLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private BufferLookup<VerdantShrineBuffer> shrineLookup;
        private NativeQueue<OpenTerminalRPC> queue;
        private int timer;
        protected override void OnCreate()
        {
            propertiesLookup = SystemAPI.GetComponentLookup<ObjectPropertiesCD>();
            objLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            plantLookup = SystemAPI.GetComponentLookup<PlantCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            shrineLookup = SystemAPI.GetBufferLookup<VerdantShrineBuffer>();
            queue = new(Allocator.Persistent);
            RequireForUpdate<VerdantShrineBuffer>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<VerdantShrineTerminalCD>(out Entity terminal))
                return;
            var ecb = CreateCommandBuffer();
            /*var queue = this.queue;
            Entities.ForEach((Entity e, in OpenTerminalRPC rpc) =>
            {
                if ((TerminalType)rpc.TerminalType == TerminalType.VerdantShrine)
                {
                    queue.Enqueue(rpc);
                    ecb.DestroyEntity(e);
                }
            })
                .WithName("VerdantShrineTerminal_CheckOpen")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var open))
            {
                ecb.AddComponent(terminal, new OpenTerminalCD()
                {
                    Player = open.Player
                });
            }

            Entities.ForEach((Entity e, ref ObjectDataCD objData, in OpenTerminalCD open) =>
            {
                PlayerController.AddSkill(open.Player, SkillID.Gardening, objData.amount - 1, ecb, true);
                objData.amount = 1;
                ecb.RemoveComponent<OpenTerminalCD>(e);
            })
               .WithName("VerdantShrineTerminal_GetExp")
               .WithAll<VerdantShrineTerminalCD>()
               .WithBurst()
               .Schedule();*/
            if(timer < 180)
            {
                timer++;
            }
            else
            {
                timer = 0;
                Entities.ForEach((Entity e, ref ObjectDataCD objData, in DistanceToPlayerCD dis) =>
                {
                    PlayerController.AddSkill(dis.closestPlayer, SkillID.Gardening, objData.amount - 1, ecb, true);
                    objData.amount = 1;
                })
                   .WithName("VerdantShrineTerminal_GetExp")
                   .WithAll<VerdantShrineTerminalCD>()
                   .WithBurst()
                   .Schedule();
            }

            var terminalData = objLookup.GetRefRW(terminal);
            containerLookup.TryGetBuffer(terminal, out var containers);
            shrineLookup.TryGetBuffer(terminal, out var shrines);
            var propertiesLookup = this.propertiesLookup;
            var plantLookup = this.plantLookup;
            var databaseLocal = database;
            var deltaTime = World.Time.DeltaTime;
            bool harvest = ModConfig.IsEnable(EnhanceCategory.Automation, EC_Automation.Plant);
            Entities.ForEach((Entity e, ref GrowingCD growing, in ObjectDataCD objData, in LocalTransform trans) =>
            {
                bool hover = true;
                int nature = 10, sea = 10, desert = 10;
                /*foreach (var info in shrines)
                {
                    var shrine = info.shrine;
                    if (ShrineHovering(shrine, info.trans, trans))
                    {
                        hover = true;
                        nature = math.max(nature, shrine.Nature);
                        sea = math.max(sea, shrine.Sea);
                        desert = math.max(desert, shrine.Desert);
                    }
                }*/

                if (!hover)
                    return;

                if (propertiesLookup.TryGetComponent(e, out var properties)
                    && growing.HasFinishedGrowing(properties) && plantLookup.TryGetComponent(e, out var plant))
                {
                    if (harvest)
                    {
                        var rng = PugRandom.GetRng();
                        sea = math.min(sea, 10);
                        int extra = rng.NextInt(10) < sea ? 1 : 0;
                        ItemHelper.PutItemToContainer(containers, plant.objectToDropWhenHarvested, plant.numberOfPlantsToDrop + extra);

                        desert = math.min(desert, 10);
                        bool gold = objData.objectID != ObjectID.GrubKapokPlant && rng.NextInt(100) < desert;
                        EntityUtility.CreateEntity(ecb, trans.Position, objData.objectID - 1, 1, databaseLocal, gold ? 1 : 0);
                        ecb.DestroyEntity(e);

                        terminalData.ValueRW.amount++;
                        return;
                    }
                    else
                    {
                        if (growing.grownTime >= 600)
                        {
                            growing.grownTime = 0;
                            var rng = PugRandom.GetRng();
                            sea = math.min(sea, 10);
                            int extra = rng.NextInt(10) < sea ? 1 : 0;
                            plantLookup.GetRefRW(e).ValueRW.numberOfPlantsToDrop += 1 + extra;
                            terminalData.ValueRW.amount++;
                            return;
                        }
                        else
                        {
                            growing.grownTime += deltaTime;
                        }
                    }
                }
                nature = math.min(nature, 10);
                growing.grownTime += deltaTime * nature / 10;
            })
                .WithName("VerdantShrine_Effect")
                .WithNone<RootPlantCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        private static bool ShrineHovering(VerdantShrineCD shrine, LocalTransform origin, LocalTransform target)
        {
            int r = shrine.radiums;
            int2 o = origin.Position.RoundToInt2();
            int2 t = target.Position.RoundToInt2();
            return t.x > o.x - r && t.x <= o.x + r && t.y > o.y - r && t.y <= o.y + r;
        }
    }
}
