using Assets.CoreEnhance.Scripts.Cores;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Quick
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(RunSimulationSystemGroup))]
    [UpdateAfter(typeof(AffectObjectWhenMelodyPlayedSystem))]
    public partial class OpenLockedMelodySystem : PugSimulationSystemBase
    {
        private EntityQuery query;
        private ComponentLookup<LocalTransform> transLookup;
        private ComponentLookup<EquippedObjectCD> equipLookup;
        protected override void OnCreate()
        {
            EntityQueryDesc entityQueryDesc = new()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<PlayerGhost>(),
                    ComponentType.ReadOnly<LocalTransform>()
                },
                None = new ComponentType[] { ComponentType.ReadOnly<DisablePhysicsCD>() }
            };
            query = EntityManager.CreateEntityQuery(entityQueryDesc);
            transLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            equipLookup = SystemAPI.GetComponentLookup<EquippedObjectCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (!EnhanceConfig.IsEnable(EnhanceCategory.OpenLockedMelody))
                return;
            var ecb = CreateCommandBuffer();
            var players = query.ToEntityArray(Allocator.Temp);
            var database = this.database;
            var transLookup = this.transLookup;
            var equipLookup = this.equipLookup;
            Entities.ForEach((Entity entity, ref AffectObjectWhenMelodyPlayedCD aCD, in ObjectDataCD objData, in LocalTransform trans) =>
            {
                if (!aCD.listening)
                    return;
                var hearRange = aCD.hearRange;
                var position = trans.Position;
                foreach (var player in players)
                {
                    if (!transLookup.TryGetComponent(player, out var pos))
                        continue;
                    float3 origin = PugDatabase.GetEntityLocalCenter(objData.objectID, database, objData.variation, 0f) + position;
                    if (math.distance(pos.Position, origin) > hearRange)
                        continue;
                    if (!equipLookup.TryGetComponent(player, out var equip))
                        continue;
                    var contained = equip.containedObject;
                    if (PugDatabase.GetEntityObjectInfo(contained.objectID, database, contained.variation).objectType != ObjectType.Instrument)
                        continue;
                    if (!aCD.timer.isRunning || aCD.timer.isTimerElapsed)
                    {
                        aCD.melodyProgress++;
                        aCD.timer.Start(0.25f, true);
                    }
                }
            })
                 .WithName("QuickOpenLockedMelody")
                 .WithBurst()
                 .WithDisposeOnCompletion(players)
                 .Schedule();
            base.OnUpdate();
        }
    }
}
