using PugMod;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.MyTest
{
    public class MyTestMod : IMod
    {
        public void EarlyInit()
        {
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data))
            {
                if (data.ObjectInfo.objectID == ObjectID.EventTerminal)
                {
                    Debug.Log(data.objectInfo.variation);
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
            if (Input.GetKeyDown(KeyCode.K))
            {
                GetEntitiesAt(Manager.main.player.WorldPosition.RoundToInt2(), out var entities);
                Debug.Log(string.Join("\n", entities));
            }
        }
        private static void GetEntitiesAt(int2 position, out List<ObjectID> entities)
        {
            entities = new List<ObjectID>();
            var entityManager = API.Client.World.EntityManager;
            var queryDesc = new EntityQueryDesc
            {
                All = new[]
                    { ComponentType.ReadOnly<ObjectDataCD>(), ComponentType.ReadOnly<LocalTransform>() },
                None = new[] { ComponentType.ReadOnly<PlayerGhost>() }
            };
            var query = entityManager.CreateEntityQuery(queryDesc);
            var array = query.ToEntityArray(Allocator.Temp);
            foreach (var entity2 in array)
            {
                var transform = entityManager.GetComponentData<LocalTransform>(entity2);
                var objData = entityManager.GetComponentData<ObjectDataCD>(entity2);
                var actualPosition = transform.Position.RoundToInt2();
                if (position.Equals(actualPosition))
                    entities.Add(objData.objectID);
            }

            array.Dispose();
        }
    }
}
