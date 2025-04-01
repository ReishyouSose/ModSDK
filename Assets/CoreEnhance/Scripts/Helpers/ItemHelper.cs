using CoreLib.Util.Extensions;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class ItemHelper
    {
        private readonly static NativeHashMap<int, bool> stackable = new(1024, Allocator.Persistent);
        public static ObjectID GetObjectID(string name) => API.Authoring.GetObjectID("CoreEnhance:" + name);
        public static ContainedObjectsBuffer CreateItem(ObjectID objID, int amount, int variation = 0)
        {
            return new()
            {
                objectData = new()
                {
                    objectID = objID,
                    amount = amount,
                    variation = variation,
                }
            };
        }
        public static bool IsStackable(this ObjectID objID)
        {
            int id = (int)objID;
            if (!stackable.TryGetValue(id, out bool stack))
            {
                stackable.Add(id, stack = PugDatabase.GetObjectInfo(objID).isStackable);
            }
            return stack;
        }

        public static void PutItemToContainer(DynamicBuffer<ContainedObjectsBuffer> containers,
            ObjectDataCD objData, int start = 0)
            => PutItemToContainer(containers, objData.objectID, objData.amount, start);

        public static void PutItemToContainer(DynamicBuffer<ContainedObjectsBuffer> containers,
            ObjectID objID, int amount = 1, int start = 0)
        {
            bool stackable = objID.IsStackable();
            int waitAmount = amount;
            for (int i = start; i < containers.Length; i++)
            {
                var info = containers[i];
                if (info.objectID == ObjectID.None)
                {
                    containers[i] = CreateItem(objID, amount);
                    return;
                }
                else
                {
                    if (info.objectID != objID)
                        continue;
                    if (stackable)
                    {
                        int total = info.amount + waitAmount;
                        if (total > 9999)
                        {
                            containers[i] = CreateItem(objID, 9999);
                            waitAmount = total - 9999;
                            continue;
                        }
                        else
                        {
                            containers[i] = CreateItem(objID, total);
                            return;
                        }
                    }
                }
            }
        }

        public static void LogComponent(this GameObject authoringData, ObjectID target)
        {
            if (authoringData.GetEntityObjectID() == target)
            {
                int count = authoringData.GetComponentCount();
                Debug.Log(count);
                for (int i = 0; i < count; i++)
                {
                    Debug.Log(authoringData.GetComponentAtIndex(i));
                }
            }
        }
        public static bool HasComponent<T>(this GameObject authoringData) where T : MonoBehaviour
        {
            return authoringData.TryGetComponent<T>(out _);
        }
    }
}
