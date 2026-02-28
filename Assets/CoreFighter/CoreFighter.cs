using Assets.CoreFighter.Scripts.Cores;
using Assets.CoreFighter.Scripts.Systems;
using PugMod;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreFighter
{
    public class CoreFighter : IMod
    {
        public void EarlyInit()
        {
            FighterConfig.Load();
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += NoRecoil;
            authoring.OnObjectTypeAdded += AttackSpeedModifierSystem.RecordOriginATKSpeed;
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

        private void NoRecoil(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (!FighterConfig.IsEnable(FighterCategory.NoRecoil))
                return;
            if (authoringData.TryGetComponent<MoveFreelyWeaponAuthoring>(out var weapon))
            {
                ref var move = ref weapon.moveSpeedMultiplier;
                move = math.max(move, 1f);
            }
        }
    }
}
