using Assets.CoreFighter.Scripts.Configs;
using Assets.CoreFighter.Scripts.Systems;
using CoreLib.Util.Extensions;
using I2.Loc;
using PugMod;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreFighter.Scripts
{
    public class CoreFighterMod : IMod
    {
        private Dictionary<float, HashSet<ObjectID>> atkSpeeds;
        public void EarlyInit()
        {
            FighterConfig.Load();
            atkSpeeds = new();
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
            if (!FighterConfig.IsEnable(FighterCategory.Misc, FC_Equip.NoRecoil))
                return;
            if (authoringData.TryGetComponent<WeaponAuthoring>(out var weapon))
            {
                ref var move = ref weapon.moveSpeedMultiplier;
                move = math.max(move, 1f);
            }
        }
    }
}
