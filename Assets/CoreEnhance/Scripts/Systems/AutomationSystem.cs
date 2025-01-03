using Assets.CoreEnhance.Scripts.Configs;
using CoreLib.Data.Configuration;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class AutomationSystem : PugSimulationSystemBase
    {
        private int simulationTickRateForPlatform;
        protected override void OnCreate()
        {
            simulationTickRateForPlatform = NetworkingManager.GetSimulationTickRateForPlatform();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var manager = EntityManager;
            var ecb = CreateCommandBuffer();
            var localDataBase = database;
            Automation_Salvage(localDataBase, manager, ecb);
            base.OnUpdate();
        }
        private void Automation_Salvage(BlobAssetReference<PugDatabase.PugDatabaseBank> localDataBase, EntityManager manager, EntityCommandBuffer ecb)
        {
            if (!ModConfig.TryGetValues(EnhanceCategory.Automation, EC_Automation.Salvage, out var values))
                return;
            int checkCount = ((ConfigEntry<int>)values["Amount"]).Value;
            int time = ((ConfigEntry<int>)values["Timer"]).Value * simulationTickRateForPlatform;
            ComponentLookup<LevelCD> lvLookup = SystemAPI.GetComponentLookup<LevelCD>();
            ComponentLookup<DurabilityCD> drLookup = SystemAPI.GetComponentLookup<DurabilityCD>();
            Entities.ForEach((DynamicBuffer<ContainedObjectsBuffer> containers,
                ref ObjectDataCD objdata, in LocalTransform local) =>
            {
                if (objdata.objectID != ObjectID.SalvageAndRepairStation)
                    return;
                int check = 0;
                for (int i = 0; i < 6; i++)
                {
                    ContainedObjectsBuffer item = containers[i];
                    if (item.objectID != ObjectID.None)
                    {
                        check++;
                    }
                }
                if (check < checkCount)
                {
                    objdata.amount = 0;
                    return;
                }
                objdata.amount++;
                if (objdata.amount < time)
                    return;
                NativeHashMap<int, int> result = new(4, Allocator.Temp);
                int scrapPart = (int)ObjectID.ScrapPart;
                for (int i = 0; i < 6; i++)
                {
                    var item = containers[i].objectData;
                    if (item.objectID == ObjectID.None)
                        continue;

                    Entity entity = PugDatabase.GetPrimaryPrefabEntity(item.objectID, localDataBase, item.variation);
                    int stack = 1, level = 1;
                    if (lvLookup.TryGetComponent(entity, out var lv))
                        level = lv.level;
                    else
                        stack = item.amount;
                    float durability = drLookup.TryGetComponent(entity, out var dr) ? (item.amount / (float)dr.maxDurability) : 1;

                    int partCount = (int)math.round(math.max(1, level * dr.repairCostMultiplier * 4));
                    partCount *= stack;
                    if (result.ContainsKey(scrapPart))
                        result[scrapPart] += partCount;
                    else
                        result.Add(scrapPart, partCount);

                    durability = math.clamp(durability, 0.3f, 0.49f);
                    ref var recipe = ref PugDatabase.GetEntityObjectInfo(item.objectID, localDataBase, 0).requiredObjectsToCraft;
                    int rc = recipe.Length;
                    for (int j = 0; j < rc; j++)
                    {
                        int count = (int)math.round(recipe[j].amount * durability * stack);
                        if (count > 0)
                        {
                            ObjectID objectID = recipe[j].objectID;
                            int id = (int)objectID;
                            if (result.ContainsKey(id))
                                result[id] += count;
                            else
                                result.Add(id, count);
                        }
                    }

                    containers[i] = new()
                    {
                        objectData = new()
                        {
                            objectID = ObjectID.None,
                            amount = 0
                        }
                    };
                }
                using (NativeHashMap<int, int>.Enumerator enumerator = result.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        KVPair<int, int> kvpair = enumerator.Current;
                        EntityUtility.CreateAndDropItem((ObjectID)kvpair.Key, 0, kvpair.Value,
                            local.Position, ecb.CreateEntity(), localDataBase, ecb);
                    }
                }
                result.Dispose();
            })
                .WithAll<CraftingCD>()
                .Schedule();
        }
    }
}
