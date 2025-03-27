using Assets.CoreEnhance.Scripts.Buffers;
using Assets.CoreEnhance.Scripts.Configs;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using CoreLib.Data.Configuration;
using PugProperties;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Automation
{
    [UpdateAfter(typeof(VerdantShrineSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class VerdantShrineTerminalSystem : PugSimulationSystemBase
    {
        private ComponentLookup<ObjectPropertiesCD> propertiesLookup;
        private ComponentLookup<DistanceToPlayerCD> disLookup;
        private ComponentLookup<PlantCD> plantLookup;
        private BufferLookup<ContainedObjectsBuffer> containerLookup;
        private BufferLookup<VerdantShrineBuffer> shrineLookup;
        protected override void OnCreate()
        {
            propertiesLookup = SystemAPI.GetComponentLookup<ObjectPropertiesCD>();
            disLookup = SystemAPI.GetComponentLookup<DistanceToPlayerCD>();
            plantLookup = SystemAPI.GetComponentLookup<PlantCD>();
            containerLookup = SystemAPI.GetBufferLookup<ContainedObjectsBuffer>();
            shrineLookup = SystemAPI.GetBufferLookup<VerdantShrineBuffer>();
            RequireForUpdate<VerdantShrineBuffer>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<VerdantShrineTerminalCD>(out Entity terminal))
                return;
            float expChance = EnhanceConfig.TryGetValue<int>(EnhanceCategory.Automation,
                EC_Automation.GiveExp, out var exp, "Gardening") ? exp.Value : 0;
            var ecb = CreateCommandBuffer();
            var deltaTime = World.Time.DeltaTime;
            containerLookup.TryGetBuffer(terminal, out var containers);
            shrineLookup.TryGetBuffer(terminal, out var shrines);
            var propertiesLookup = this.propertiesLookup;
            var plantLookup = this.plantLookup;
            var disLookup = this.disLookup;
            var databaseLocal = database;
            bool harvest = EnhanceConfig.TryGetValues(EnhanceCategory.Automation, EC_Automation.Plant, out var values);
            int green = (values["Nature"] as ConfigEntry<int>).Value;
            int blue = (values["Sea"] as ConfigEntry<int>).Value;
            int red = (values["Desert"] as ConfigEntry<int>).Value;
            Entities.ForEach((Entity e, ref GrowingCD growing, in ObjectDataCD objData, in LocalTransform trans) =>
            {
                bool hover = false;
                int nature = 0, sea = 0, desert = 0;
                foreach (var info in shrines)
                {
                    if (ShrineHovering(info, trans))
                    {
                        hover = true;
                        nature = math.max(nature, info.Nature);
                        sea = math.max(sea, info.Sea);
                        desert = math.max(desert, info.Desert);
                    }
                }

                if (!hover)
                    return;

                nature = math.min(green, nature);
                sea = math.min(blue, sea);
                desert = math.min(red, desert) * 3;

                if (propertiesLookup.TryGetComponent(e, out var properties)
                    && growing.HasFinishedGrowing(properties) && plantLookup.TryGetComponent(e, out var plant))
                {
                    disLookup.TryGetComponent(terminal, out var dis);
                    if (harvest)
                    {
                        var rng = PugRandom.GetRng();
                        int extra = 0;
                        for (int i = 0; i < sea; i++)
                        {
                            extra += rng.NextInt(10) == 0 ? 1 : 0;
                        }
                        ItemHelper.PutItemToContainer(containers, plant.objectToDropWhenHarvested, plant.numberOfPlantsToDrop + extra);
                        EntityUtility.CreateEntity(ecb, trans.Position, objData.objectID - 1, 1, databaseLocal,
                            objData.objectID != ObjectID.GrubKapokPlant && rng.NextInt(100) < desert ? 1 : 0);
                        if (rng.NextInt(100) < expChance)
                            PlayerController.AddSkill(dis.closestPlayer, SkillID.Gardening, 1, ecb, true);
                        ecb.DestroyEntity(e);
                        return;
                    }
                    else
                    {
                        if (growing.grownTime >= 600)
                        {
                            var rng = PugRandom.GetRng();
                            growing.grownTime = 0;
                            int extra = 0;
                            for (int i = 0; i < sea; i++)
                            {
                                extra += rng.NextInt(10) == 0 ? 1 : 0;
                            }
                            ItemHelper.PutItemToContainer(containers, plant.objectToDropWhenHarvested, 1 + extra);
                            if (rng.NextInt(100) < expChance)
                                PlayerController.AddSkill(dis.closestPlayer, SkillID.Gardening, 1, ecb, true);
                            return;
                        }
                        else
                        {
                            growing.grownTime += deltaTime * (nature / 10f + 1);
                        }
                    }
                }
                growing.grownTime += deltaTime * nature / 10;
            })
                .WithName("VerdantShrine_Effect")
                .WithNone<RootPlantCD>()
                .WithBurst()
                .Schedule();
            base.OnUpdate();
        }
        private static bool ShrineHovering(VerdantShrineBuffer shrine, LocalTransform target)
        {
            int r = shrine.radiums;
            int2 o = shrine.trans.Position.RoundToInt2();
            int2 t = target.Position.RoundToInt2();
            return t.x > o.x - r && t.x <= o.x + r && t.y > o.y - r && t.y <= o.y + r;
        }
    }
}
