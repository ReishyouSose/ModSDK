using Assets.CoreFighter.Scripts.Cores;
using Assets.CoreFighter.Scripts.Systems.Equip;
using PugMod;
using UnityEngine;

namespace Assets.CoreFighter
{
    public class CoreFighter : IMod
    {
        public void EarlyInit()
        {
            new FighterConfig().Register();
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += NoRecoilSystem.RecordOriginMoveSpeed;
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
    }
}
