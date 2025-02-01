using PugMod;
using Unity.Collections;

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

        [GenerateTestsForBurstCompatibility]
        public static bool IsStackable(this ObjectID objID)
        {
            int id = (int)objID;
            if (!stackable.TryGetValue(id, out bool stack))
            {
                stackable.Add(id, stack = PugDatabase.GetObjectInfo(objID).isStackable);
            }
            return stack;
        }
    }
}
