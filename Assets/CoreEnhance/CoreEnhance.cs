using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Assets.CoreEnhance.Scripts.Systems.Automation;
using Assets.CoreEnhance.Scripts.Systems.Infinity;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using Assets.CoreEnhance.Scripts.UI.ItemLookup;
using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.Entity;
using CoreLib.Submodule.UserInterface;
using Pug.Automation;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance
{
    public class CoreEnhance : IMod
    {
        public const string InternalName = "CoreEnhance:";
        private int timer;
        public void EarlyInit()
        {
            var authoring = API.Authoring;
            authoring.OnObjectTypeAdded += AutoDoorSystem.MarkDoor;
            authoring.OnObjectTypeAdded += InfinityArenaSystem.MarkArena;
            authoring.OnObjectTypeAdded += DataModify.AuthoringModify;
            authoring.OnObjectTypeAdded += ObtainLookupUI.CheckData;
            authoring.OnObjectTypeAdded += ContainerDisplaySystem.MarkHighLight;
            authoring.OnObjectTypeAdded += Test;
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule)/*,typeof(EntityModule)*/);
            ModKeyBind.Load();
        }

        private void Test(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            if (authoringData.GetEntityObjectID(out int variation) == ObjectID.HeartBerry)
            {
                Debug.Log(variation);
                var extract = authoringData.GetComponent<ExtractableAuthoring>();
                foreach (var ex in extract.extractedObject)
                {
                    Debug.Log((ex.objectID, ex.minMaxRandomAmountOverride));
                }
            }
            if (authoringData.GetEntityObjectID(out _) == ObjectID.FishingNet)
            {
                var crafts = authoringData.GetComponent<CraftingAuthoring>();
                Debug.LogWarning("fishing net craft: " + crafts.canCraftObjects.Count);
                foreach (var craft in crafts.canCraftObjects)
                {
                    Debug.Log((craft.objectID, craft.hasPrerequisites));
                }
            }
        }

        public void Init()
        {
            BurstDisabler.DisableBurstForSystem<PugAutomationFishingSystem>();
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Shutdown()
        {
        }

        public void Update()
        {
            var p = Manager.main.player;
            if (p == null)
                return;
            ModKeyBind.Handle(p);
            if (timer < 60)
            {
                timer++;
                return;
            }
            timer = 0;
        }
    }
}
