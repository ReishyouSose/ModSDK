using CoreLib.Util.Extensions;
using PugMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImproveEquipmentMod : IMod
{
    internal static ModConfig config;
    public void EarlyInit()
    {
        config = new();
        API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
    }

    private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
    {
        /*if (ComponentCheck(authoringData, out var cps, ObjectID.LaserDrillTool, ObjectID.LightningGun))
        {
            foreach (var cp in cps)
            {
                if (cp is WeaponAuthoring weapon)
                {
                    Debug.Log(weapon.);
                }
            }
        }*/
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
