using Inventory;
using Unity.Collections;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class ContainerHelper
    {
        [GenerateTestsForBurstCompatibility]
        public static NativeHashSet<int> GetExistObjects(InventoryHandlerShared shared, Entity entity)
        {
            NativeHashSet<int> existObject = new(80, Allocator.Temp);
            if (!shared.containedObjectsBufferLookup.TryGetBuffer(entity, out var slots) ||
             !shared.inventoryLookup.TryGetBuffer(entity, out var invs))
                return existObject;
            foreach (var inv in invs)
            {
                var end = inv.startIndex + inv.size;
                for (int i = inv.startIndex; i < end; i++)
                {
                    var item = slots[i];
                    var id = item.objectID;
                    if (id == ObjectID.None)
                        continue;
                    existObject.Add((int)id);
                }
            }
            return existObject;
        }

        [GenerateTestsForBurstCompatibility]
        public static void QuickStack(in InventoryHandlerShared shared, Entity player, Entity container)
        {
            if (!shared.lockedObjectsBufferLookup.TryGetBuffer(player, out var lockStats) ||
                !shared.containedObjectsBufferLookup.TryGetBuffer(player, out var slots) ||
                !shared.inventoryLookup.TryGetBuffer(player, out var invs))
                return;

            var existObjects = GetExistObjects(shared, container);
            foreach (var inv in invs)
            {
                var end = inv.startIndex + inv.size;
                for (int i = inv.startIndex; i < end; i++)
                {
                    if (lockStats[i].Value)
                        continue;
                    var item = slots[i];
                    if (item.objectID == ObjectID.None)
                        continue;
                    if (!existObjects.Contains((int)item.objectID))
                        continue;
                    if (!InventoryUtility.TryMoveAll(shared, player, i, container, -1, -1, int.MaxValue, false))
                        return;
                }
            }
            existObjects.Dispose();
        }

        [GenerateTestsForBurstCompatibility]
        public static void Replenish(in InventoryHandlerShared shared, Entity player, Entity container)
        {
            if (!shared.containedObjectsBufferLookup.TryGetBuffer(container, out var slots) ||
                !shared.inventoryLookup.TryGetBuffer(container, out var invs))
                return;

            var existObjects = GetExistObjects(shared, player);
            foreach (var inv in invs)
            {
                var end = inv.startIndex + inv.size;
                for (int i = inv.startIndex; i < end; i++)
                {
                    var item = slots[i];
                    if (item.objectID == ObjectID.None)
                        continue;
                    if (!existObjects.Contains((int)item.objectID))
                        continue;
                    if (!InventoryUtility.TryMoveAll(shared, container, i, player, -1, -1, int.MaxValue, false))
                        return;
                }
            }
            existObjects.Dispose();
        }

        [GenerateTestsForBurstCompatibility]
        public static void PutAll(in InventoryHandlerShared shared, Entity player, Entity container)
        {
            if (!shared.lockedObjectsBufferLookup.TryGetBuffer(player, out var locks) ||
                !shared.containedObjectsBufferLookup.TryGetBuffer(player, out var slots) ||
                !shared.inventoryLookup.TryGetBuffer(player, out var invs))
                return;

            foreach (var inv in invs)
            {
                var end = inv.startIndex + inv.size;
                for (int i = inv.startIndex; i < end; i++)
                {
                    if (locks[i].Value)
                        continue;
                    var item = slots[i];
                    if (item.objectID == ObjectID.None)
                        continue;
                    if (!InventoryUtility.TryMoveAll(shared, player, i, container, -1, -1, int.MaxValue, false))
                        return;
                }
            }
        }

        [GenerateTestsForBurstCompatibility]
        public static void TakeAll(in InventoryHandlerShared shared, Entity player, Entity container)
        {
            if (!shared.containedObjectsBufferLookup.TryGetBuffer(container, out var slots) ||
                !shared.inventoryLookup.TryGetBuffer(container, out var invs))
                return;
            foreach (var inv in invs)
            {
                var end = inv.startIndex + inv.size;
                for (int i = inv.startIndex; i < end; i++)
                {
                    var item = slots[i];
                    if (item.objectID == ObjectID.None)
                        continue;
                    if (!InventoryUtility.TryMoveAll(shared, container, i, player, -1, -1, int.MaxValue, false))
                        return;
                }
            }
        }

        [GenerateTestsForBurstCompatibility]
        public static void SplitStacks(in InventoryHandlerShared shared, Entity container)
        {
            if (!shared.containedObjectsBufferLookup.TryGetBuffer(container, out var slots) ||
                !shared.inventoryLookup.TryGetBuffer(container, out var invs))
                return;

            foreach (var inv in invs)
            {
                int start = inv.startIndex;
                int end = start + inv.size;

                for (int i = start; i < end;)
                {
                    int originalIndex = i; 
                    var originalItem = slots[originalIndex];
                    var id = originalItem.objectID;

                    if (id == ObjectID.None || !id.IsStackable() || originalItem.amount <= 1)
                    {
                        i++;
                        continue;
                    }

                    int splitCount = 0;
                    int nextPos = i + 1;

                    while (splitCount < originalItem.amount - 1 && nextPos < end)
                    {
                        if (slots[nextPos].objectID == ObjectID.None)
                        {
                            slots[nextPos] = new ContainedObjectsBuffer
                            {
                                objectData = new ObjectDataCD
                                {
                                    objectID = id,
                                    variation = originalItem.variation,
                                    amount = 1
                                }
                            };
                            splitCount++;
                        }
                        else
                        {
                            i = nextPos;
                            break;
                        }
                        nextPos++;
                    }

                    slots[originalIndex] = new ()
                    {
                        objectData = new ()
                        {
                            objectID = id,
                            variation = originalItem.variation,
                            amount = originalItem.amount - splitCount
                        }
                    };
                }
            }
        }
    }
}
