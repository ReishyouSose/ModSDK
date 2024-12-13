using Assets.InfiniteArena;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.InfinieArena
{
    public class InfinieArenaMod : IMod
    {
        private static ModConfig config;
        internal static ModConfig Config => config;
        public void EarlyInit()
        {
            config = new();
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (authoringData.TryGetComponent(out EntityMonoBehaviourData objData))
            {
                var info = objData.objectInfo;
                if (info.objectID != ObjectID.EventTerminal || info.variation != 1)
                    return;
                entityManager.AddComponentData(entity, new DistanceToPlayerCD());
                entityManager.AddComponentData(entity, new ArenaRecordCD());
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
