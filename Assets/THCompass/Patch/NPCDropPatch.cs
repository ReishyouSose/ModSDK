using Assets.THCompass.DataStruct;
using Assets.THCompass.Helper;
using HarmonyLib;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using static DropLootSystem;

namespace Assets.THCompass.Patch
{
    [HarmonyPatch]
    public static class NPCDropPatch
    {

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(LootTableBank), "GetLootTables")]
        private static void AddExtraLoots_Vanilla(LootTableBank __instance, List<LootTable> __result)
        {
            //AddExtraLoots_Boss(__result);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LootTableConverter), "Convert")]
        private static bool AddExtraLoots_Mods(LootTableConverter __instance, LootTableAuthoring authoring)
        {
            //AddExtraLoots_Boss(Manager.mod.LootTable);
            return true;
        }*/
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DropLootSystem), "DropLootSystem_8A6AD24_LambdaJob_3_Execute")]
        private static bool DropLootSystem_3(DropLootSystem __instance, EntityCommandBuffer ecb, BlobAssetReference<PugDatabase.PugDatabaseBank> databaseLocal, int playerCount, uint lootSeed, UnsafeList<LootChest> lootChests)
        {
            for (int i = 0; i < lootChests.Length; i++)
            {
                LootChest lootChest = lootChests[i];
                DynamicBuffer<ContainedObjectsBuffer> containedObjectsBuffer = lootChest.containedObjectsBuffer;
                int index = 0;
                bool bird = false;
                for (int j = 0; j < containedObjectsBuffer.Length; j++)
                {
                    var contained = containedObjectsBuffer[j];
                    ObjectDataCD item = contained.objectData;
                    if (item.objectID == ObjectID.None)
                    {
                        index = j;
                        break;
                    }
                    if (item.objectID == ObjectID.PileOfChum)
                    {
                        bird = true;
                    }
                }
                if (MatchCompass(lootChest.chestObjectData.objectID, bird, out ObjectID compass))
                {
                    containedObjectsBuffer[index] = new()
                    { objectData = ItemHelper.NewItem(compass, GetRngAmount(playerCount)) };
                    lootChest.containedObjectsBuffer = containedObjectsBuffer;
                }
            }
            return true;
        }
        private static bool MatchCompass(in ObjectID chest, in bool bird, out ObjectID compass)
        {
            compass = ObjectID.None;
            if (bird)
            {
                compass = ItemHelper.GetItemID("Compass_Bird");
                return true;
            }
            BossID bossID = chest switch
            {
                ObjectID.GlurchChest => BossID.Slime,
                ObjectID.HivemotherHalloweenChest => BossID.Hive,
                ObjectID.HivemotherChest => BossID.Hive,
                ObjectID.GhormChest => BossID.Devourer,
                ObjectID.InventoryAncientChest => BossID.Shaman,
                ObjectID.IvyChest => BossID.PoisonSlime,
                ObjectID.EasterChest => BossID.Bird,
                ObjectID.MorphaChest => BossID.SlipperySlime,
                ObjectID.OctopusBossChest => BossID.Octopus,
                ObjectID.LavaSlimeBossChest => BossID.LavaSlime,
                ObjectID.BossChest => BossID.Scarab,
                ObjectID.AtlantianWormChest => BossID.Atlantis,
                _ => BossID.None
            };
            if (bossID == BossID.None) return false;
            compass = ItemHelper.GetItemID("Compass_" + bossID);
            return true;
        }
        private static int GetRngAmount(int playerCount)
        {
            int amount = 0;
            for (int j = 0; j < playerCount; j++)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (PugRandom.GetRng().NextInt(i) == 0)
                    {
                        amount++;
                    }
                }
                if (PugRandom.GetRng().NextInt(1000) == 0)
                {
                    amount += 99;
                }
            }
            return amount;
        }
        /*private static  void InitLoot(List<LootInfo> lootInfos, int minUniqueDrops, int maxUniqueDrops, List<LootInfo> guaranteedLootInfos)
        {
            float num = 0f; // 初始化总权重为0
            foreach (LootInfo lootInfo3 in lootInfos) // 遍历所有的LootInfo
            {
                num += lootInfo3.weight; // 累加权重
            }

            float num2 = 0f; // 初始化累加掉落概率为0
            float num3 = (float)(maxUniqueDrops + minUniqueDrops) / 2f; // 计算掉落数量的平均值
            foreach (LootInfo lootInfo in lootInfos) // 再次遍历所有的LootInfo
            {
                float num4 = ((num <= 0f) ? 0f : (lootInfo.weight / num)); // 计算该物品的权重占比
                float num5 = (1f - math.pow(1f - num4, num3)) * 100f; // 根据掉落数量和权重占比计算掉落概率
                lootInfo.editorVisualDropChance = num5; // 将计算出的掉落概率存储到LootInfo中
                float num6 = 0f; // 初始化保底掉落数量为0
                if (guaranteedLootInfos != null && lootInfo.isPartOfGuaranteedDrop) // 如果有保底掉落且该物品是保底掉落之一
                {
                    LootInfo lootInfo2 = guaranteedLootInfos.Find((LootInfo x) => x.objectID == lootInfo.objectID); // 找到对应的保底掉落
                    lootInfo.editorVisualDropChance = (1f - (1f - lootInfo2.editorVisualDropChance / 100f) * (1f - num5 / 100f)) * 100f; // 根据保底掉落和普通掉落的概率计算最终掉落概率
                    num6 = lootInfo2.editorVisualDropChance / 100f * ((float)(lootInfo2.amount.min + lootInfo2.amount.max) / 2f) * 100f; // 计算保底掉落的数量
                }

                float num7 = num4 * ((float)(lootInfo.amount.min + lootInfo.amount.max) / 2f) * num3 * 100f + num6; // 计算该物品的期望掉落数量
                lootInfo.info = lootInfo.editorVisualDropChance.ToString(CultureInfo.InvariantCulture) + "%       Amount per hundred " + num7.ToString(CultureInfo.InvariantCulture); // 将掉落概率和期望掉落数量存储到LootInfo中
                num2 += num4; // 累加权重占比
                lootInfo.accumulatedDropChance = num2; // 将累加的权重占比存储到LootInfo中
            }
        }
        */
    }
}
