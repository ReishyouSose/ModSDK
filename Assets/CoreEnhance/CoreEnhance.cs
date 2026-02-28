using Assets.CoreEnhance.Scripts.Cores;
using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Systems.Automation;
using Assets.CoreEnhance.Scripts.Systems.Infinity;
using Assets.CoreEnhance.Scripts.Systems.Misc;
using Assets.CoreEnhance.Scripts.UI.ItemLookup;
using CoreLib;
using CoreLib.Submodule.ControlMapping;
using CoreLib.Submodule.UserInterface;
using PugMod;
using Rewired;
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
            authoring.OnObjectTypeAdded += Test;
            authoring.OnObjectTypeAdded += ObtainLookupUI.CheckData;
            authoring.OnObjectTypeAdded += ContainerDisplaySystem.MarkHighLight;
            CoreLibMod.LoadSubmodule(typeof(ControlMappingModule));
            ModKeyBind.Load();
        }

        private void Test(Entity entity, GameObject authoringData, EntityManager entityManager)
        {
            authoringData.LogComponent(ObjectID.HeartBerry);
            authoringData.LogComponent(ObjectID.FishingNet);
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
