using Inventory;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ResetPetSkinRPC : IRpcCommand
    {
        public Entity Player;
    }
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PetModifierClient : PugSimulationSystemBase
    {
        private static PetModifierClient ins;
        private NativeQueue<Entity> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(ResetPetSkinRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var player))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, new ResetPetSkinRPC() { Player = player });
            }
            base.OnUpdate();
        }
        public static void ResetSkin(Entity player) => ins.queue.Enqueue(player);
    }
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PetModifierServer : PugSimulationSystemBase
    {
        private ComponentLookup<PetCD> petLookup;
        private ComponentLookup<EquipmentCD> equipLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        protected override void OnCreate()
        {
            petLookup = SystemAPI.GetComponentLookup<PetCD>();
            equipLookup = SystemAPI.GetComponentLookup<EquipmentCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingletonBuffer<InventoryChangeBuffer>(out var invChangeBuffer))
            {
                var ecb = CreateCommandBuffer();
                var database = this.database;
                var petLookup = this.petLookup;
                var equipLookup = this.equipLookup;
                var containerLookup = this.containerLookup;
                Entities.ForEach((Entity e, in ResetPetSkinRPC rpc) =>
                {
                    var player = rpc.Player;
                    int index = equipLookup[player].petIndex;
                    invChangeBuffer.Add(new()
                    {
                        inventoryChangeData = Create.ConsumeObjectType(player, ObjectID.AncientCoin, 200),
                        playerEntity = player
                    });
                    ObjectID id = containerLookup[player][index].objectID;
                    Entity pet = PugDatabase.GetPrimaryPrefabEntity(id, database);
                    invChangeBuffer.Add(new InventoryChangeBuffer()
                    {
                        inventoryChangeData = new()
                        {
                            inventoryAction = InventoryAction.SetPetSkin,
                            index1 = index,
                            index2 = PugRandom.GetRng().NextInt(petLookup[pet].maxSkins),
                            objectID = id,
                            inventory1 = player
                        },
                        playerEntity = player
                    });
                    ecb.DestroyEntity(e);
                })
                    .WithName("PetModifier")
                    .WithAll<ReceiveRpcCommandRequest>()
                    .WithBurst()
                    .Schedule();
            }
            base.OnUpdate();
        }
    }
}
