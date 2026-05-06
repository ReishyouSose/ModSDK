using Assets.CoreFighter.Scripts.Cores;
using PlayerEquipment;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreFighter.Scripts.Systems.Equip
{
    public enum ItemType
    {
        None,
        Tool,
        Melee,
        Range,
        Magic,
        Throw,
        Beam,
        OffHand,
        Consume
    }
    public struct OriginCoolDownCD : IComponentData
    {
        public int ItemType;
        public float CoolDown;
    }

    [UpdateInGroup(typeof(EquipmentUpdateSystemGroup))]
    [UpdateBefore(typeof(EquipmentUpdateSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial class AttackSpeedModifierSystem : PugSimulationSystemBase
    {
        private float timer;
        private ComponentLookup<CooldownCD> cdLookup;
        private ComponentLookup<OriginCoolDownCD> originCDLookup;
        protected override void OnCreate()
        {
            cdLookup = SystemAPI.GetComponentLookup<CooldownCD>();
            originCDLookup = SystemAPI.GetComponentLookup<OriginCoolDownCD>();
            NeedDatabase();
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
            bool enable = FighterConfig.ATKSpeedModifierIsEnable;
            var database = this.database;
            var modifiers = FighterConfig.GetModifiers();
            var cdLookup = this.cdLookup;
            var originCDLookup = this.originCDLookup;
            var job = Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> inv,
                in EquippedObjectCD held, in EquipmentCD equip) =>
            {
                ModifierATKSpeed(inv, held.equippedSlotIndex, enable, database, originCDLookup, modifiers, cdLookup);
                ModifierATKSpeed(inv, equip.offHandIndex, enable, database, originCDLookup, modifiers, cdLookup);
            })
                .ScheduleParallel(Dependency);
            job.Complete();
            modifiers.Dispose();
            base.OnUpdate();
        }
        private static void ModifierATKSpeed(DynamicBuffer<ContainedObjectsBuffer> inv, int index, bool enable, BlobAssetReference<PugDatabase.PugDatabaseBank> database, ComponentLookup<OriginCoolDownCD> originCDLookup, NativeHashMap<float, ATKSpeedModifer> modifiers, ComponentLookup<CooldownCD> cdLookup)
        {
            Entity e = PugDatabase.GetPrimaryPrefabEntity(inv[index].objectID, database);
            if (!originCDLookup.TryGetComponent(e, out var origin))
                return;
            ref var cd = ref cdLookup.GetRefRW(e).ValueRW;
            var oriCD = origin.CoolDown;
            if (!enable)
            {
                cd.cooldown = oriCD;
                return;
            }
            if (!modifiers.TryGetValue(oriCD, out var modifier))
                return;
            if (!modifier.Switch)
            {
                cd.cooldown = oriCD;
                return;
            }
            cd.cooldown = (ItemType)origin.ItemType switch
            {
                ItemType.Tool => modifier.Tool,
                ItemType.Melee => modifier.Melee,
                ItemType.Range => modifier.Range,
                ItemType.Magic => modifier.Magic,
                ItemType.Throw => modifier.Throw,
                ItemType.Beam => modifier.Beam,
                ItemType.OffHand => modifier.OffHand,
                ItemType.Consume => modifier.Consume,
                _ => oriCD
            };
        }
        internal static void RecordOriginATKSpeed(Entity e, GameObject authoringData, EntityManager manager)
        {
            if (authoringData.TryGetComponent<CooldownAuthoring>(out var cd))
            {
                if (cd.cooldown == 0)
                    return;
                ObjectType type = ObjectType.NonUsable;
                if (authoringData.TryGetComponent(out EntityMonoBehaviourData data))
                    type = data.objectInfo.objectType;
                else if (authoringData.TryGetComponent(out ObjectAuthoring obj))
                    type = obj.objectType;
                ItemType itemType = type switch
                {
                    ObjectType.MiningPick or ObjectType.Sledge or ObjectType.Hoe
                       or ObjectType.Shovel or ObjectType.DrillTool => ItemType.Tool,
                    ObjectType.MeleeWeapon => ItemType.Melee,
                    ObjectType.RangeWeapon => authoringData.GetComponent<WeaponDamageAuthoring>()
                                               .isMagic ? ItemType.Magic : ItemType.Range,
                    ObjectType.BeamWeapon => ItemType.Beam,
                    ObjectType.ThrowingWeapon => ItemType.Throw,
                    ObjectType.Offhand => ItemType.OffHand,
                    ObjectType.Eatable => ItemType.Consume,
                    _ => ItemType.None,
                };
                manager.AddComponentData(e, new OriginCoolDownCD()
                {
                    CoolDown = cd.cooldown,
                    ItemType = (int)itemType
                });
            }
        }
    }
}
