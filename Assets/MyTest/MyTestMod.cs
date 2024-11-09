using PugMod;
using UnityEngine;

namespace Assets.MyTest
{
    public class MyTestMod : IMod
    {
        public void EarlyInit()
        {
            //API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data))
            {
                if (data.ObjectInfo.objectID == ObjectID.HeartBerrySeed)
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
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }
    }
}
