using Unity.Entities;
using Unity.Transforms;

namespace Assets.PlantAutonomyTheory
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class HarvestSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            NeedDatabase();
            RequireForUpdate<TileCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var lookup = GetAttackHelper().propertiesLookup;
            var tileAccessor = CreateTileAccessor();
            var dataBaseLocal = database;
            var config = PlantAutonomyTheoryMod.Config;
            bool autoMode = config.AutoMode;
            int regrowthTimer = config.RegrowthTimer;
            int harvestCycel = config.HarvestCycel;
            int harvestCycel_Golden = config.HarvestCycel_Golden;

            Entities.ForEach((Entity entity, ref GrowingCD grow, ref PlantCD plant, in ObjectDataCD objData, in LocalTransform local) =>
            {
                if (!grow.FullyGrown(entity, lookup) || !tileAccessor.ArableWet(local))
                    return;
                if (autoMode)
                {
                    if (grow.grownTime < regrowthTimer)
                        return;
                    EntityUtility.CreateAndDropItem(plant.objectToDropWhenHarvested, 0,
                        plant.numberOfPlantsToDrop, local.Position, ecb.CreateEntity(), dataBaseLocal, ecb);
                    grow.grownTime = -1;
                }
                else
                {
                    bool golden = plant.GoldenPlant(objData);
                    if (grow.grownTime < (golden ? harvestCycel_Golden : harvestCycel))
                        return;
                    plant.numberOfPlantsToDrop++;
                    grow.grownTime = 0;
                }
            })
                .WithName("Harvest")
                .WithNone<RootPlantCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class GrowingSystem : PugSimulationSystemBase
    {
        protected override void OnCreate()
        {
            NeedDatabase();
            RequireForUpdate<TileCD>();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var ecb = CreateCommandBuffer();
            var tileAccessor = CreateTileAccessor();
            var lookup = GetAttackHelper().propertiesLookup;
            var dataBaseLocal = database;
            int chance = GoldenLevelServer.max * 3;
            float deltaTime = World.Time.DeltaTime;

            Entities.ForEach((Entity entity, ref GrowingCD grow, ref ObjectDataCD objData, in LocalTransform local) =>
            {
                if (grow.FullyGrown(entity, lookup))
                {
                    if (grow.grownTime < 0)
                    {
                        EntityUtility.CreateEntity(ecb, local.Position, objData.objectID - 1, 1,
                            dataBaseLocal, (PugRandom.GetRng().NextInt(100) < chance) ? 1 : 0);
                        ecb.DestroyEntity(entity);
                    }
                    else if (tileAccessor.ArableWet(local))
                        grow.grownTime += deltaTime;
                }
            })
                .WithName("Growing")
                .WithNone<RootPlantCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
    }
}
