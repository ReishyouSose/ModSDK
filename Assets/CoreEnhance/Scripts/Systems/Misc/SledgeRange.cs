using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using PlayerEquipment;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct SledgeCD : IComponentData { }

    [UpdateInGroup(typeof(EquipmentUpdateSystemGroup))]
    [UpdateBefore(typeof(EquipmentUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class SledgeRangeSystem : PugSimulationSystemBase
    {
        private float timer;
        private ComponentLookup<SledgeCD> sledgeLookup;
        private ComponentLookup<MeleeWeaponCD> meleeLookup;
        protected override void OnCreate()
        {
            sledgeLookup = SystemAPI.GetComponentLookup<SledgeCD>();
            meleeLookup = SystemAPI.GetComponentLookup<MeleeWeaponCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (timer < 1)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            bool enable = EnhanceConfig.TryGetValue<float>(EnhanceCategory.ModifySledgeRange, out var value);
            var sledgeLookup = this.sledgeLookup;
            var meleeLookup = this.meleeLookup;
            float range = value.Value;
            Entities.ForEach((in EquippedObjectCD held) =>
            {
                Entity e = held.equipmentPrefab;
                if (!sledgeLookup.HasComponent(e))
                    return;
                ref var melee = ref meleeLookup.GetRefRW(e).ValueRW;
                melee.baseHitColliderSize = enable ? range : 1.4f;
            })
                .WithName("SledgeRangeModify")
                .WithAll<PlayerGhost>()
                .WithBurst()
                .ScheduleParallel(Dependency);
            base.OnUpdate();
        }
        public static void MarkSledge(Entity e, GameObject authoringData, EntityManager entityManager)
        {
            if (!authoringData.GetEntityObjectID(out _).ToString().Contains("Sledge"))
                return;
            entityManager.AddComponent<SledgeCD>(e);
        }
    }
}
