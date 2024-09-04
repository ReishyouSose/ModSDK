using PugMod;
using PugTilemap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.THCompass.Helper
{
    public static class ItemHelper
    {
        public static ObjectID GetItemID(string ItemName) => API.Authoring.GetObjectID("THCompass:" + ItemName);
        public static void DropItem(ObjectID itemType, int stack = 1, float3? pos = null)
        {
            API.Server.DropObject((int)itemType, 0, stack, pos ?? Manager.main.player.WorldPosition);
        }
        public static void DropItem(string ItemName, int stack = 1, float3? pos = null)
        {
            DropItem(GetItemID(ItemName), stack, pos ?? Manager.main.player.WorldPosition);
        }
        public static void DropItem(this PlayerController p, string ItemName, int stack = 1)
        {
            DropItem(ItemName, stack, p.WorldPosition);
        }
        public static void DropItem(this PlayerController p, ObjectID itemType, int stack = 1)
        {
            DropItem(itemType, stack, p.WorldPosition);
        }
        /// <summary>
        /// <see cref="BiomeRanges.GetBiomeAtPosition(int2, NativeArray{BiomeRanges})"/>
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="tileset">(Tileset)top.tileset</param>
        /// <returns></returns>
        public static ObjectID BiomeAndTilesetToChest(Biome biome, Tileset tileset)
        {
            switch (biome)
            {
                case Biome.Slime:
                    if (tileset == Tileset.Dirt || tileset == Tileset.Turf || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedCopperChest;
                    }

                    break;
                case Biome.Larva:
                    if (tileset == Tileset.Clay || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedCopperChest;
                    }

                    break;
                case Biome.Stone:
                    if (tileset == Tileset.Stone || tileset == Tileset.Sand)
                    {
                        return ObjectID.LockedIronChest;
                    }

                    break;
                case Biome.Nature:
                    switch (tileset)
                    {
                        case Tileset.Stone:
                        case Tileset.Nature:
                            return ObjectID.LockedScarletChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
                case Biome.Sea:
                    switch (tileset)
                    {
                        case Tileset.Sea:
                            return ObjectID.LockedOctarineChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
                case Biome.Desert:
                    switch (tileset)
                    {
                        case Tileset.Desert:
                            return ObjectID.LockedGalaxiteChest;
                        case Tileset.Crystal:
                            return ObjectID.LockedSolariteChest;
                    }

                    break;
            }

            return ObjectID.None;
        }
        public static bool TryGetComponent<T>(this ObjectID itemType, out T component) where T : unmanaged, IComponentData
        {
            component = default;
            if (PugDatabase.HasComponent<T>(itemType))
            {
                component = PugDatabase.GetComponent<T>(itemType);
                return true;
            }
            return false;
        }
        public static bool TryGetComponent<T>(this ObjectDataCD item, out T component) where T : unmanaged, IComponentData
        {
            component = default;
            if (PugDatabase.HasComponent<T>(item))
            {
                component = PugDatabase.GetComponent<T>(item);
                return true;
            }
            return false;
        }
        public static ObjectInfo GetObjectInfo(this Entity entity, World world) => EntityUtility.GetObjectInfo(entity, world);
        public static ObjectDataCD NewItem(ObjectID id, int amount) => new() { amount = amount, objectID = id };
    }
}
