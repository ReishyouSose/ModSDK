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
            "ArenaScanner"
        };
        internal static void EditWorkbench(Entity entity, GameObject authoring, EntityManager entityManager)
        {
            ObjectID objectID = authoring.GetEntityObjectID();
            if (objectID == ObjectID.SolariteWorkbench)
            {
                var list = WorkBench;
                var canCraftBuffer = entityManager.GetBuffer<CanCraftObjectsBuffer>(entity);
                int j = 0;
                for (int i = 8; i < 12; i++)
                {
                    var item = API.Authoring.GetObjectID(CoreEnhance + list[j++]);
                    canCraftBuffer[i] = new CanCraftObjectsBuffer
                    {
                        objectID = item,
                        amount = 1,
                    };
                }
                canCraftBuffer[2] = new()
                {
                    objectID = API.Authoring.GetObjectID(CoreEnhance + list[j]),
                    amount = 1
                };
            }
        }
    }
}
