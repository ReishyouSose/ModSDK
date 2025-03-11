using CoreLib.Util.Extensions;
using PugMod;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Edits
{
    public static class ModRecipes
    {
        private const string CoreEnhance = "CoreEnhance:";
        private static List<string> WorkBench => new()
        {
            "AutoFisher",
            "AutoFisherTerminal",
            "VerdantShrine",
            "VerdantShrineTerminal",
            "ArenaScanner",
            "BoulderDemolish"
        };
        internal static void EditWorkbench(Entity entity, GameObject authoring, EntityManager entityManager)
        {
            ObjectID objectID = authoring.GetEntityObjectID();
            /*if (objectID == API.Authoring.GetObjectID("CoreEnhance:WorkBench"))
            {
                var canCraftBuffer = entityManager.GetBuffer<CanCraftObjectsBuffer>(entity);
                foreach (var id in WorkBench)
                {
                    canCraftBuffer.Add(new()
                    {
                        objectID = API.Authoring.GetObjectID(CoreEnhance + id),
                        amount = 1
                    });
                }
            }*/
            if (objectID == ObjectID.SolariteWorkbench)
            {
                var canCraftBuffer = entityManager.GetBuffer<CanCraftObjectsBuffer>(entity);

                canCraftBuffer[2] = new()
                {
                    objectID = API.Authoring.GetObjectID(CoreEnhance + "ArenaScanner"),
                    amount = 1
                };
            }
            if (objectID == ObjectID.LaboratoryWorkbench)
            {
                var canCraftBuffer = entityManager.GetBuffer<CanCraftObjectsBuffer>(entity);

                canCraftBuffer[10] = new()
                {
                    objectID = API.Authoring.GetObjectID(CoreEnhance + "BoulderDemolish"),
                    amount = 1
                };
            }
        }
    }
}
