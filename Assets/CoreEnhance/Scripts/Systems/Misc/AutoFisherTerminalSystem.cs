using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using static Assets.CoreEnhance.Scripts.Helpers.ItemHelper;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct AFTerminalOpenRPC : IRpcCommand
    {
        public Entity Player;
        public AFTerminalOpenRPC(Entity player) => Player = player;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AutoFisherTerminalClient : PugSimulationSystemBase
    {
        private static AutoFisherTerminalClient ins;
        private NativeQueue<AFTerminalOpenRPC> queue;
        private EntityArchetype archetype;
        private EntityQuery query;
        private float timer;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(AFTerminalOpenRPC), typeof(SendRpcCommandRequest));
            query = EntityManager.CreateEntityQuery(typeof(AutoFisherTerminalCD));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out _))
            {
                ecb.CreateEntity(archetype);
            }

            if (timer < 60)
            {
                timer += World.Time.DeltaTime;
            }
            else
            {
                timer = 0;
                var find = query.ToEntityArray(Allocator.Temp);
                if (find.Any())
                {
                    queue.Enqueue(new(Entity.Null));
                }
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
        protected override void OnCreate()
        {
            queue = new(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var queue = this.queue;
            var ecb = CreateCommandBuffer();
            Entities.ForEach((Entity e,in AFTerminalOpenRPC rpc) =>
            {
                queue.Enqueue(rpc);
                ecb.DestroyEntity(e);
            })
                .WithName("AutoFisherTerminal_CheckOpen")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();

            while (queue.TryDequeue(out var open))
            {
                bool isPlayerOpen = open.Player != Entity.Null;
                NativeHashMap<int, int> loot = new(128, Allocator.Temp);
                if (isPlayerOpen)
                {
                    loot.Add(-1, 0);
                }
                JobHandle checkLoot = Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers, ref ObjectDataCD objData) =>
                {
                    if (isPlayerOpen)
                    {
                        loot[-1] += objData.amount - 1;
                        objData.amount = 1;
                    }
                    int count = containers.Length;
                    for (int i = 9; i < count; i++)
                    {
                        var item = containers[i];
                        var objID = item.objectID;
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
                Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers) =>
                {
                    if (dispose)
                        return;
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
                    dispose = true;

                    int length = containers.Length;
                    int count = drops.Length;
                    for (int j = 0; j < count; j++)
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
                     .WithName("AutoFisherTerminal_PutLoot")
                     .WithAll<AutoFisherTerminalCD>()
                     .WithBurst()
                     .ScheduleParallel(checkLoot);
            }
            base.OnUpdate();
        }
    }
}
