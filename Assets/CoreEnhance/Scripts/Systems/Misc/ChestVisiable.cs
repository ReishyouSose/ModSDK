using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    public struct ChestInfo
    {
        public float3 pos;
        public ObjectDataCD objData;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class ChestVisiableSystem : PugSimulationSystemBase
    {
        internal static ChestVisiableSystem Ins { get; private set; }
        public NativeList<ChestInfo> infos;
        protected override void OnCreate()
        {
            Ins = this;
            infos = new(Allocator.Persistent);
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var player = Manager.main.player;
            if (player == null)
                return;
            var infos = this.infos;
            infos.Clear();
            var selected = Manager.ui.currentSelectedUIElement;
            if (selected == null)
                return;
            var target = selected.GetContainedObject().objectID;
            if (target == ObjectID.None)
                return;
            var playerP = player.transform.position;
            float dis = math.pow(20, 2);
            Entities.ForEach((Entity e, in DynamicBuffer<ContainedObjectsBuffer> containers,
                in ObjectDataCD objData, in LocalTransform trans) =>
            {
                foreach (var container in containers)
                {
                    if (container.objectID == target)
                    {
                        infos.Add(new()
                        {
                            pos = trans.Position,
                            objData = objData
                        });
                    }
                }
            })
                .WithName("ChestVisiable")
                .WithNone<PlayerGhost>()
                .WithBurst()
                .Run();
        }
    }
}
