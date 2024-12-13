using CoreLib;
using CoreLib.Submodules.ModEntity;
using PugMod;
using UnityEngine;

namespace Assets.PlantAutonomyTheory
{
    public class PlantAutonomyTheoryMod : IMod
    {
        private static ModConfig config;
        internal static ModConfig Config => config ??= new();
        public void EarlyInit()
        {
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
            CoreLibMod.LoadModules(typeof(EntityModule));
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data))
            {
                if (data.ObjectInfo.objectID == ObjectID.RuinsPedestal)
                {
                    int count = authoringData.GetComponentCount();
                    for (int i = 0; i < count; i++)
                    {
                        var comp = authoringData.GetComponentAtIndex(i);
                        Debug.Log(comp);
                    }
                }
            }
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            if (obj is not GameObject gameObject)
                return;

            var entityMono = gameObject.GetComponent<EntityMonoBehaviour>();
            if (entityMono != null)
            {
                EntityModule.EnablePooling(gameObject);
            }
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                ObjectID id = API.Authoring.GetObjectID("PlantAutonomyTheory:GardeningAltarEntity");
                API.Server.DropObject((int)id, 0, 9999, Manager.main.player.WorldPosition);
            }
        }
    }
}
