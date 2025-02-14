using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using static Assets.CoreEnhance.Scripts.Helpers.ItemHelper;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    public struct AFTerminalOpenRPC : IRpcCommand
    {
        public Entity Player;
        public AFTerminalOpenRPC(Entity player) => Player = player;
    }
    public struct AFTerminalOpenCD : IComponentData, IEnableableComponent
    {
        public Entity Player;
        public AFTerminalOpenCD(Entity player) => Player = player;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AutoFisherTerminalClient : PugSimulationSystemBase
    {
        private static AutoFisherTerminalClient ins;
        private NativeQueue<AFTerminalOpenRPC> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(AFTerminalOpenRPC), typeof(SendRpcCommandRequest));
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
        public static void OpenAFTerminal()
        {
            ins.queue.Enqueue(new(Manager.main.player.entity));
        }
    }

    [UpdateAfter(typeof(UniquePlaceableSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutoFisherTerminalServer : PugSimulationSystemBase
    {
        private NativeQueue<AFTerminalOpenRPC> queue;
        private ComponentLookup<AFTerminalOpenCD> openLookup;
        private ComponentLookup<ObjectDataCD> objDataLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private EntityQuery query;
        private float timer;
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            openLookup = SystemAPI.GetComponentLookup<AFTerminalOpenCD>();
            objDataLookup = SystemAPI.GetComponentLookup<ObjectDataCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            query = EntityManager.CreateEntityQuery(typeof(AutoFisherCD));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var queue = this.queue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e, in AFTerminalOpenRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(e);
            })
                .WithName("AutoFisherTerminal_CheckOpen")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            var openLookup = this.openLookup;

            if (timer < 60)
            {
                timer += World.Time.DeltaTime;
            }
            else
            {
                timer = 0;
                SetOpen(ecb, openLookup, Entity.Null);
            }
            while (queue.TryDequeue(out var open))
            {
                SetOpen(ecb, openLookup, open.Player);
            }

            var query = this.query;
            var containerLookup = this.containerLookup;
            var objDataLookup = this.objDataLookup;
            Entities.ForEach((Entity e, DynamicBuffer<ContainedObjectsBuffer> containers, in AFTerminalOpenCD open) =>
            {
                openLookup.SetComponentEnabled(e, false);
                bool isPlayerOpen = open.Player != Entity.Null;
                NativeHashMap<int, int> loot = new(128, Allocator.Temp);
                if (isPlayerOpen)
                {
                    loot.Add(-1, 0);
                }
                using (var autoFisheres = query.ToEntityArray(Allocator.Temp))
                {
                    foreach (var autoFisher in autoFisheres)
                    {
                        if (isPlayerOpen)
                        {
                            ref var objData = ref objDataLookup.GetRefRW(autoFisher).ValueRW;
                            loot[-1] += objData.amount - 1;
                            objData.amount = 1;
                        }

                        containerLookup.TryGetBuffer(autoFisher, out var contents);
                        int count = contents.Length;
                        for (int i = 9; i < count; i++)
                        {
                            var item = contents[i];
                            var objID = item.objectID;
                            int id = (int)item.objectID;
                            if (loot.ContainsKey(id))
                                loot[id] += item.amount;
                            else
                                loot[id] = item.amount;
                            contents[i] = CreateItem(ObjectID.None, 0);
                        }
                    }
                }
                NativeList<ObjectDataCD> drops = new(Allocator.Temp);
                foreach (var info in loot)
                {
                    if (info.Key == -1)
                    {
                        PlayerController.AddSkill(open.Player, SkillID.Fishing, info.Value, ecb, true);
                    }
                    else
                    {
                        drops.Add(new()
                        {
                            objectID = (ObjectID)info.Key,
                            amount = info.Value
                        });
                    }
                }
                loot.Dispose();
                int length = containers.Length;
                int DRcount = drops.Length;
                for (int j = 0; j < DRcount; j++)
                {
                    var item = drops[j];
                    ObjectID origin = item.objectID;
                    if (origin.IsStackable())
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
                .WithName("AutoFisherTerminal_CollectLoots")
                .WithAll<AutoFisherTerminalCD>()
                .WithBurst()
                .Schedule();

            base.OnUpdate();
        }
        private void SetOpen(EntityCommandBuffer ecb, ComponentLookup<AFTerminalOpenCD> openLookup, Entity player)
        {
            Entities.ForEach((Entity e) =>
            {
                openLookup.SetComponentEnabled(e, true);
                ecb.SetComponent<AFTerminalOpenCD>(e, new(player));
            })
                .WithName("AutoFisherTerminal_SetOpen")
                .WithAll<AutoFisherTerminalCD>()
                .WithNone<AFTerminalOpenCD>()
                .WithBurst()
                .Schedule();
        }
    }
}
