using CoreLib.Util.Extensions;
using PugMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.ImproveEquipment
{
    public class ImproveEquipmentMod : IMod
    {
        internal static ModConfig config;
        public void EarlyInit()
        {
            config = new();
            //API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            ObjectID id = authoringData.GetEntityObjectID();
            if (ComponentCheck(authoringData, out var cps, ObjectID.AzeosSoulOrb))
            {
                foreach (var cp in cps)
                {
                    Debug.Log(cp);
                    /*if(cp is ObjectAuthoring weapon)
                    {
                        Debug.Log(weapon.objectType);
                    }*/
                }
            }
        }

        public void Init()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
        }
        private static bool ComponentCheck(GameObject authoringData, out List<Component> cps, params ObjectID[] ids)
        {
            ObjectID id = authoringData.GetEntityObjectID();
            cps = new();
            if (ids.Contains(id))
            {
                Debug.Log(id);
                int count = authoringData.GetComponentCount();
                for (int i = 0; i < count; i++)
                {
                    var cp = authoringData.GetComponentAtIndex(i);
                    cps.Add(cp);
                    Debug.Log(cp);
                }
                return true;
            }
            return false;
        }
    }
}
