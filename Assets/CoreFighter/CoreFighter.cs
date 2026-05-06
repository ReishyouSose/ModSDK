using Assets.CoreFighter.Scripts.Cores;
using Assets.CoreFighter.Scripts.Systems.Equip;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreFighter
{
    public class CoreFighter : IMod
    {
        public void EarlyInit()
        {
            new FighterConfig().Register();
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            NoRecoilSystem.RecordOriginMoveSpeed(entity, authoringData, entityManager);
            AttackSpeedModifierSystem.RecordOriginATKSpeed(entity, authoringData, entityManager);
            if (authoringData.TryGetComponent(out UseModProjectileID use))
            {
                var range = entityManager.GetComponentData<RangeWeaponCD>(entity);
                range.projectileID = API.Authoring.GetObjectID(use.ProjectileID);
                entityManager.SetComponentData(entity, range);
            }
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }
    }
}
