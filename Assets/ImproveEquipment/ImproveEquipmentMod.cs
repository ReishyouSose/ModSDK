using CoreLib.Util.Extensions;
using PugMod;
using System.Collections.Generic;
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
            if (ComponentCheck(authoringData, out var cps))
            {
                foreach (var cp in cps)
                {
                    //Debug.Log(cp);
                    if (cp is EntityMonoBehaviourData authoring && authoring.objectInfo.objectType == ObjectType.PlayerType)
                    {
                        Debug.Log(id);
                    }
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
            // if (Input.GetKey(KeyCode.K))
            {
                //CompanionEntityBuffer
            }
        }
        private static bool ComponentCheck(GameObject authoringData, out List<Component> cps, params ObjectID[] ids)
        {
            ObjectID id = authoringData.GetEntityObjectID();
            cps = new();
            //if (ids.Contains(id))
            {
                //Debug.Log(id);
                int count = authoringData.GetComponentCount();
                for (int i = 0; i < count; i++)
                {
                    var cp = authoringData.GetComponentAtIndex(i);
                    cps.Add(cp);
                    //Debug.Log(cp);
                }
                return true;
            }
            return false;
        }
    }
}
