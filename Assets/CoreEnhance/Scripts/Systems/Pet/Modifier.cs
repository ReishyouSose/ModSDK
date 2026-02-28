using Assets.CoreEnhance.Scripts.Cores;
using Inventory;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct PetModifierRPC : IRpcCommand
    {
        public PetModifyID Modify;
        public Entity Player;
        public int data1;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PetModifierClient : PugSimulationSystemBase
    {
        private static PetModifierClient ins;
        private NativeQueue<PetModifierRPC> queue;
        private EntityArchetype archetype;
        protected override void OnCreate()
        {
            ins = this;
            queue = new(Allocator.Persistent);
            archetype = EntityManager.CreateArchetype(typeof(PetModifierRPC), typeof(SendRpcCommandRequest));
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            while (queue.TryDequeue(out var rpc))
            {
                var e = ecb.CreateEntity(archetype);
                ecb.SetComponent(e, rpc);
            }
            base.OnUpdate();
        }
        public static void ModifyPet(PetModifyID modify, Entity player, int data1 = 0)
        {
            if (!(modify switch
            {
                PetModifyID.ResetSkin => EnhanceConfig.IsEnable(EnhanceCategory.ResetSkin),
                PetModifyID.RollAllSkill or PetModifyID.RollSingleSkill
                    => EnhanceConfig.IsEnable(EnhanceCategory.RollSkill),
                _ => false
            }))
                return;
            ins.queue.Enqueue(new PetModifierRPC() { Modify = modify, Player = player, data1 = data1 });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class PetModifierServer : PugSimulationSystemBase
    {
        private ComponentLookup<PetCD> petLookup;
        private ComponentLookup<PetSkinCD> petSkinLookup;
        private ComponentLookup<PetOwnerCD> petOwnerLookup;
        private BufferLookup<PetTalentBuffer> petTalentLookup;
        private BufferLookup<PetTalentPoolBuffer> petTalentPoolLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private InventoryAuxDataSystem invSystem;
        protected override void OnCreate()
        {
            petLookup = SystemAPI.GetComponentLookup<PetCD>();
            petSkinLookup = SystemAPI.GetComponentLookup<PetSkinCD>();
            petOwnerLookup = SystemAPI.GetComponentLookup<PetOwnerCD>();
            petTalentLookup = SystemAPI.GetBufferLookup<PetTalentBuffer>();
            petTalentPoolLookup = SystemAPI.GetBufferLookup<PetTalentPoolBuffer>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            invSystem = World.GetExistingSystemManaged<InventoryAuxDataSystem>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<InventoryChangeBuffer>(out var invChangeBuffer))
                return;
            var ecb = CreateCommandBuffer();
            var database = this.database;
            var petLookup = this.petLookup;
            var petSkinLookup = this.petSkinLookup;
            var petOwnerLookup = this.petOwnerLookup;
            var petTalentLookup = this.petTalentLookup;
            var petTalentPoolLookup = this.petTalentPoolLookup;
            var containerLookup = this.containerLookup;
            var invSystem = this.invSystem.SystemData;
            var manager = EntityManager;
            Entities.ForEach((Entity e, in PetModifierRPC rpc) =>
            {
                ecb.DestroyEntity(e);
                var player = rpc.Player;
                int index = petOwnerLookup[player].SlotIndex;
                var objBuffer = containerLookup[player][index];
                ObjectID id = objBuffer.objectID;
                int auxIndex = objBuffer.auxDataIndex;
                Entity pet = PugDatabase.GetPrimaryPrefabEntity(id, database);
                var rng = PugRandom.GetRng();
                var accessor = invSystem.GetAccessor();
                switch (rpc.Modify)
                {
                    case PetModifyID.ResetSkin:
                        if (!InventoryUtility.HasObject(containerLookup, player, ObjectID.AncientCoin, 200))
                            return;
                        int max = petLookup[pet].maxSkins;
                        if (max < 2)
                            return;
                        if (!accessor.TryGetComponentData(auxIndex, petSkinLookup, out var skinCD))
                            return;
                        var oldSkin = skinCD.skinIndex;
                        var skin = oldSkin;
                        while (skin == oldSkin)
                            skin = rng.NextInt(max);
                        invChangeBuffer.Add(new InventoryChangeBuffer()
                        {
                            inventoryChangeData = new()
                            {
                                inventoryAction = InventoryAction.SetPetSkin,
                                index1 = index,
                                index2 = skin,
                                objectID = id,
                                inventory1 = player
                            },
                            playerEntity = player
                        });
                        invChangeBuffer.Add(new()
                        {
                            inventoryChangeData = Create.ConsumeObjectType(player, ObjectID.AncientCoin, 200),
                            playerEntity = player
                        });
                        break;
                    case PetModifyID.RollAllSkill:
                        if (!InventoryUtility.HasObject(containerLookup, player, ObjectID.AncientCoin, 200))
                            return;
                        if (!petTalentPoolLookup.TryGetBuffer(pet, out var skills))
                            return;
                        if (!accessor.TryGetBuffer(auxIndex, petTalentLookup, out var talents))
                            return;
                        for (int i = 0; i < 9; i++)
                        {
                            talents[i] = new PetTalentBuffer
                            {
                                petTalentID = skills[rng.NextInt(skills.Length)].petTalentID
                            };
                        }
                        invChangeBuffer.Add(new InventoryChangeBuffer()
                        {
                            inventoryChangeData = new()
                            {
                                inventoryAction = InventoryAction.ResetPetTalentTree,
                                index1 = index,
                                bool1 = true,
                                inventory1 = player
                            },
                            playerEntity = player
                        });
                        invChangeBuffer.Add(new()
                        {
                            inventoryChangeData = Create.ConsumeObjectType(player, ObjectID.AncientCoin, 200),
                            playerEntity = player
                        });
                        break;
                    case PetModifyID.RollSingleSkill:
                        int skillIndex = rpc.data1;
                        if (skillIndex < 0)
                            return;
                        if (!InventoryUtility.HasObject(containerLookup, player, ObjectID.PetCandyEpic, 100))
                            return;
                        if (!petTalentPoolLookup.TryGetBuffer(pet, out skills))
                            return;
                        if (!accessor.TryGetBuffer(auxIndex, petTalentLookup, out talents))
                            return;
                        var old = talents[skillIndex];
                        var oldTalent = old.petTalentID;
                        var talent = oldTalent;
                        while (talent == oldTalent)
                            talent = skills[rng.NextInt(skills.Length)].petTalentID;
                        int point = old.points;
                        talents[skillIndex] = new PetTalentBuffer
                        {
                            petTalentID = talent,
                            points = point
                        };
                        invChangeBuffer.Add(new InventoryChangeBuffer()
                        {
                            inventoryChangeData = new()
                            {
                                inventoryAction = InventoryAction.SetPetTalentPoints,
                                index1 = index,
                                index2 = skillIndex,
                                amount = point,
                                objectID = id,
                                inventory1 = player
                            },
                            playerEntity = player
                        });
                        invChangeBuffer.Add(new()
                        {
                            inventoryChangeData = Create.ConsumeObjectType(player, ObjectID.PetCandyEpic, 100),
                            playerEntity = player
                        });
                        break;
                }
            })
                .WithName("PetModifier")
                .WithAll<ReceiveRpcCommandRequest>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
