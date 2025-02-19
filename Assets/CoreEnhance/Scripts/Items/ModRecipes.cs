using CoreLib.Util.Extensions;
using PugMod;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    public static class ModRecipes
    {
        private const string CoreEnhance = "CoreEnhance:";
        private static List<string> WorkBench()
        {
            return new()
            {
                "AutoFisher",
                "AutoFisherTerminal",
                "VerdantShrine",
                "VerdantShrineTerminal",
                "ArenaScanner"
            };
        }
        internal static void EditWorkbench(Entity entity, GameObject authoring, EntityManager entityManager)
        {
            ObjectID objectID = authoring.GetEntityObjectID();
            if (objectID == API.Authoring.GetObjectID(CoreEnhance + "WorkBench"))
            {
                var canCraftBuffer = entityManager.GetBuffer<CanCraftObjectsBuffer>(entity);
                int i = 0;
                foreach (var name in WorkBench())
                {
                    var item = API.Authoring.GetObjectID(CoreEnhance + name);
                    canCraftBuffer[i++] = new CanCraftObjectsBuffer
                    {
                        objectID = item,
                        amount = 1,
                        entityAmountToConsume = 0
                    };
                }
            }
        }
    }
}
