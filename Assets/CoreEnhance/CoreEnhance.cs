using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Systems.Automation;
using Assets.CoreEnhance.Scripts.Systems.Infinity;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance
{
    public class CoreEnhance : IMod
    {
        public void EarlyInit()
        {
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += AutoDoorSystem.MarkDoor;
            authoring.OnObjectTypeAdded += InfinityArenaSystem.MarkArena;
            authoring.OnObjectTypeAdded += Test;
        }

        private void Test(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
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
