using PugMod;
using UnityEngine;

namespace Assets.AutoSalvage
{
    public class AutoSalvageMod : IMod
    {
        private static ModConfig config;
        internal static ModConfig Config => config ??= new();
        public void EarlyInit()
        {
            //API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }
        /*private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data))
            {
                ObjectID id = data.objectInfo.objectID;
                if (id == ObjectID.MorphaBag)
                {
                    Debug.Log("Report MorphaBag compomnent");

                    foreach (var rc in data.ObjectInfo.requiredObjectsToCraft)
                    {
                        Debug.Log((rc.objectID, rc.amount));
                    }
                    int count = authoringData.GetComponentCount();
                    for (int i = 0; i < count; i++)
                    {
                        Component c = authoringData.GetComponentAtIndex(i);
                        Debug.Log(c);
                    }
                }
            }
        }*/

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
