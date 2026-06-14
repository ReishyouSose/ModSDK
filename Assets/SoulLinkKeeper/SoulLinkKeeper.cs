using HarmonyLib;
using PugMod;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SoulLinkKeeper
{
    [HarmonyPatch]
    public class SoulLinkKeeper : IMod
    {
        internal static ChatWindow.MessageTextType PlayerDeathType;
        public void EarlyInit()
        {
            PlayerDeathType = (ChatWindow.MessageTextType)1000;
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data) && data.ObjectInfo.objectID == ObjectID.Player)
            {
                entityManager.AddComponent<DeathSpreadCD>(entity);
                entityManager.AddComponent<BlockDeathSpread>(entity);
                entityManager.SetComponentEnabled<BlockDeathSpread>(entity, false);
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

        [HarmonyPatch(typeof(ChatWindow), "Awake"), HarmonyPrefix]
        private static void AddPlayerDeathMessage(List<ChatWindow.MessageTextPrefab> ___textPrefabs)
        {
            ___textPrefabs.Add(new()
            {
                type = PlayerDeathType,
            });
        }

        [HarmonyPatch(typeof(ChatWindow), "Awake"), HarmonyPostfix]
        private static void AddPlayerDeathMessage(Dictionary<ChatWindow.MessageTextType, GameObject> ___textPrefabLookup)
        {
            var origin = ___textPrefabLookup[ChatWindow.MessageTextType.DurabilityLost];
            var obj = Object.Instantiate(origin, origin.transform.parent);
            Object.DontDestroyOnLoad(obj);
            obj.GetComponent<PugText>().SetText("SoulLinkKeeper/PlayerDead");
            ___textPrefabLookup[PlayerDeathType] = obj;
        }
    }
}
