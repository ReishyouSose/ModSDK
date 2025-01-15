using Assets.CoreEnhance.Scripts.Items;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct AutoFisherExpRpc : IRpcCommand
    {
        public Entity AutoFisher;
        public Entity Player;
        public AutoFisherExpRpc(Entity autoFisher, Entity player)
        {
            AutoFisher = autoFisher;
            Player = player;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AutoFisherClient : PugSimulationSystemBase
    {
        private NativeQueue<AutoFisherExpRpc> queue;
        private EntityArchetype archetype;
        private static AutoFisherClient ins;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(AutoFisherExpRpc), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var exp))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, exp);
            }
            base.OnUpdate();
        }
        public static void ReceiveExp(Entity autoFisher, Entity player)
        {
            ins.queue.Enqueue(new(autoFisher, player));
        }
    }

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherServer : PugSimulationSystemBase
    {
        private NativeHashMap<int, bool> stackable;
        private BiomeLookup biomeLookup;
        private ComponentLookup<ObjectDataCD> objDataLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private float moveTimer;
        private EntityQuery terminal;
        protected override void OnCreate()
        {
            stackable = new(1024, Allocator.Persistent);
            terminal = EntityManager.CreateEntityQuery(typeof(AutoFisherTerminalCD));
            NeedDatabase();
            NeedLootBank();
            base.OnCreate();
        }
        protected override void OnStartRunning()
        {
            objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            biomeLookup = SystemAPI.TryGetSingleton<BiomeSamplesCD>(out var sample)
                ? new(sample) : new(SystemAPI.GetSingleton<BiomeRangesCD>().Value, Allocator.Persistent);
            base.OnStartRunning();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var objDataLookup = this.objDataLookup;
            Entities.ForEach((Entity e, in AutoFisherExpRpc exp) =>
            {
                ref var objData = ref objDataLookup.GetRefRW(exp.AutoFisher).ValueRW;
                PlayerController.AddSkill(exp.Player, SkillID.Fishing, objData.amount, ecb, true);
                objData.amount = 0;
                ecb.DestroyEntity(e);
            })
                .WithName("AutoFisher_ReceiveExp")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            var tileAccessor = CreateTileAccessor();
            var biomeLookup = this.biomeLookup;
            var localDatabase = database;
            var localLootBack = lootBank;
            var localStackable = stackable;
            var delta = SystemAPI.Time.DeltaTime;
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData,
                ref AutoFisherCD af, ref RandomCD random, in LocalTransform trans) =>
            {
                if (!af.CheckRodLevel(containers, out float efficiency))
                    return;
                ref var rng = ref random.Value;
                float targetTime = 10f / efficiency;
                if (af.timer < targetTime)
                {
                    af.timer += delta;
                    return;
                }
                af.timer = 0;
                AutoFisherCD.Init(ref af, tileAccessor, biomeLookup, trans);
                var drops = PugDatabase.GetRandomLoot(rng.NextInt(5) == 0 ? af.items : af.fishes, 1, 1,
                      ref rng, localLootBack, localDatabase, trans.Position, af.biome);
                int count = containers.Length;
                for (int i = 9; i < count; i++)
                {
                    if (containers[i].objectData.objectID == ObjectID.None)
                    {
                        var item = drops[0];
                        containers[i] = CreateItem(item.objectID, item.amount);
                        break;
                    }
                }
                objData.amount++;
                drops.Dispose();
            })
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();

            if (moveTimer < 5)
            {
                moveTimer += delta;
                return;
            }
            moveTimer = 0;

            int exp = 0;
            NativeHashMap<int, int> loot = new(128, Allocator.Temp);
            JobHandle checkLoot = Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData) =>
            {
                if (containers[2].objectID == ObjectID.None)
                    return;
                exp += objData.amount;
                objData.amount = 0;
                int count = containers.Length;
                for (int i = 9; i < count; i++)
                {
                    var item = containers[i];
                    int id = (int)item.objectID;
                    if (loot.ContainsKey(id))
                        loot[id] += item.amount;
                    else
                        loot[id] = item.amount;
                    containers[i] = CreateItem(ObjectID.None, 0);
                }
            })
                 .WithName("AutoFisherTerminal_CheckLoot")
                 .WithAll<AutoFisherCD>()
                 .WithBurst()
                 .ScheduleParallel(Dependency);

            bool dispose = false;
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData) =>
            {
                if (dispose)
                    return;
                objData.amount += exp;
                NativeList<ObjectDataCD> drops = new(Allocator.Temp);
                foreach (var info in loot)
                {
                    drops.Add(new()
                    {
                        objectID = (ObjectID)info.Key,
                        amount = info.Value
                    });
                }
                loot.Dispose();
                dispose = true;

                int length = containers.Length;
                int count = drops.Length;
                for (int j = 0; j < count; j++)
                {
                    var item = drops[j];
                    ObjectID origin = item.objectID;
                    if (!localStackable.TryGetValue((int)origin, out bool stack))
                    {
                        ref var info = ref PugDatabase.GetEntityObjectInfo(origin, localDatabase);
                        localStackable.Add((int)origin, stack = info.isStackable);
                    }
                    if (stack)
                    {
                        int empty = -1;
                        for (int i = 0; i < length; i++)
                        {
                            var data = containers[i].objectData;
                            ObjectID id = data.objectID;
                            if (empty < 0 && id == ObjectID.None)
                            {
                                empty = i;
                            }
                            if (id == item.objectID && data.amount < 9999)
                            {
                                int amount = data.amount + item.amount;
                                if (amount > 9999)
                                {
                                    containers[i] = CreateItem(id, 9999);
                                    drops[j] = new()
                                    {
                                        objectID = id,
                                        amount = amount - 9999
                                    };
                                    continue;
                                }
                                else
                                {
                                    containers[i] = CreateItem(id, amount);
                                    break;
                                }
                            }
                        }
                        if (empty >= 0)
                        {
                            containers[empty] = CreateItem(item.objectID, item.amount);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            if (containers[i].objectData.objectID == ObjectID.None)
                            {
                                containers[i] = CreateItem(item.objectID, item.amount);
                                break;
                            }
                        }
                    }
                }
                drops.Dispose();
            })
                 .WithName("AutoFisherTerminal_PutLoot")
                 .WithAll<AutoFisherTerminalCD>()
                 .WithBurst()
                 .ScheduleParallel(checkLoot);

            base.OnUpdate();
        }
        private static ContainedObjectsBuffer CreateItem(ObjectID objID, int amount, int variation = 0)
        {
            return new()
            {
                objectData = new()
                {
                    objectID = objID,
                    amount = amount,
                    variation = variation,
                }
            };
        }
    }
}
