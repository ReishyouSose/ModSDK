using Assets.CoreFighter.Scripts.Cores;
using PlayerEquipment;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Systems.Equip
{
    public struct OriginMoveSpeedCD : IComponentData
    {
        public float Value;
    }

    [UpdateInGroup(typeof(EquipmentUpdateSystemGroup))]
    [UpdateBefore(typeof(EquipmentUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class NoRecoilSystem : PugSimulationSystemBase
    {
        private float timer;
        private ComponentLookup<MoveFreelyWeaponCD> moveLookup;
        private ComponentLookup<OriginMoveSpeedCD> originLookup;
        protected override void OnCreate()
        {
            moveLookup = SystemAPI.GetComponentLookup<MoveFreelyWeaponCD>();
            originLookup = SystemAPI.GetComponentLookup<OriginMoveSpeedCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            bool enable = FighterConfig.IsEnable(FighterCategory.NoRecoil);
            if (timer < 1)
            {
                timer += World.Time.DeltaTime;
                return;
            }
            timer = 0;
            var moveLookup = this.moveLookup;
            var originLookup = this.originLookup;
            Entities.ForEach((in EquippedObjectCD held) =>
            {
                var entity = held.equipmentPrefab;
                if (!moveLookup.HasComponent(entity) || !originLookup.TryGetComponent(entity, out var origin))
                    return;
                ref var speed = ref moveLookup.GetRefRW(entity).ValueRW.moveSpeedMultiplier;
                if (enable)
                {
                    if (speed < 1)
                        speed = 1;
                }
                else
                {
                    if (speed != origin.Value)
                        speed = origin.Value;
                }
            })
                .WithName("NoRecoil")
                .WithBurst()
                .WithAll<PlayerGhost>()
                .Schedule();
            base.OnUpdate();
        }
        public static void RecordOriginMoveSpeed(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            float value = 1f;
            if (authoringData.TryGetComponent<MoveFreelyWeaponAuthoring>(out var weapon))
                value = weapon.moveSpeedMultiplier;
            else
                entityManager.AddComponentData(entity, new MoveFreelyWeaponCD() { moveSpeedMultiplier = 1 });
            entityManager.AddComponentData(entity, new OriginMoveSpeedCD() { Value = value });
        }
    }
}
