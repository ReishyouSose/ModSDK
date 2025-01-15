using Assets.CoreEnhance.Scripts.Items;
using CoreLib.Submodules.ModEntity.Patches;
using System.Linq;
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
        private ComponentLookup<InitialMoveInventoryFromCD> moveLookup;
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
                ref AutoFisherCD af, ref RandomCD random, in InventoryCD inv, in LocalTransform trans) =>
            {
                if (!af.CheckRodLevel(containers, out float efficiency))
                    return;
                ref var rng = ref random.Value;
                float targetTime = 60f / efficiency;
                if (af.timer < targetTime)
                {
                    af.timer += delta;
                    return;
                }
                af.timer = 0;
                AutoFisherCD.Init(ref af, tileAccessor, biomeLookup, trans);
                var drops = PugDatabase.GetRandomLoot(rng.NextInt(5) == 0 ? af.items : af.fishes, 1, 1,
                      ref rng, localLootBack, localDatabase, trans.Position, af.biome);
                int length = inv.size;
                int count = drops.Length;
                for (int j = 0; j < count; j++)
                {
                    var item = drops[j];
                    objData.amount++;
                    ObjectID origin = item.objectID;
                    if (!localStackable.TryGetValue((int)origin, out bool stack))
                    {
                        ref var info = ref PugDatabase.GetEntityObjectInfo(origin, localDatabase);
                        localStackable.Add((int)origin, stack = info.isStackable);
                    }
                    if (stack)
                    {
                        int empty = -1;
                        for (int i = 9; i < length; i++)
                        {
                            var data = containers[i].objectData;
                            ObjectID id = data.objectID;
                            if (empty < 0 && id == ObjectID.None)
                            {
                                empty = i;
                            }
                            if (id == item.objectID)
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
                .WithName("AutoFisher_Catch")
                .WithBurst()
                .Schedule();

            if (moveTimer < 5)
            {
                moveTimer += delta;
                return;
            }
            moveTimer = 0;
            var terminalFind = terminal.ToEntityArray(Allocator.Temp);
            if (!terminalFind.Any())
            {
                terminalFind.Dispose();
                return;
            }
            Entity first = terminalFind.First();
            terminalFind.Dispose();
            var moveLookup = this.moveLookup;
            Entities.ForEach((Entity e, DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData) =>
            {
                if (containers[2].objectID == ObjectID.None)
                    return;
                objDataLookup.GetRefRW(first).ValueRW.amount += objData.amount;
                objData.amount = 0;
                ecb.AddComponent(first, new InitialMoveInventoryFromCD()
                {
                    entityFrom = e,
                    startSlotToMove = 9,
                    amountToMove = 36
                });
            })
                .WithName("AutoFisher_Move")
                .WithAll<AutoFisherCD>()
                .WithBurst()
                .Schedule();

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
