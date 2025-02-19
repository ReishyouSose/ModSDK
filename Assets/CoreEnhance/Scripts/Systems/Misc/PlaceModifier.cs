using PlayerEquipment;
using System;
using Unity.Entities;

namespace Assets.CoreEnhance.Scripts.Systems.Misc
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(EquipmentBeforeUpdateSystemGroup))]
    public partial class PlaceModifier : PugSimulationSystemBase
    {
        private bool init;
        private ComponentLookup<ResizableTileSizeCD> lookup;
        protected override void OnCreate()
        {
            lookup = SystemAPI.GetComponentLookup<ResizableTileSizeCD>();
            NeedDatabase();
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            if (init)
                return;
            if (database == null)
                return;
            var array = Enum.GetValues(typeof(ObjectID));
            foreach (var a in array)
            {
                var id = (ObjectID)a;
                var info = PugDatabase.GetObjectInfo(id);
                if (info?.objectType is ObjectType.Hoe or ObjectType.Shovel)
                {
                    var e = PugDatabase.GetPrimaryPrefabEntity(id, database);
                    if (!lookup.HasComponent(e))
                        continue;
                    ref var data = ref PugDatabase.GetEntityObjectInfo(id, database);
                    data.prefabTileSize = new(10, 10);
                }
            }
            init = true;
            base.OnStartRunning();
        }
    }
}
