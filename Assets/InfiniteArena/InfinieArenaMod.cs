using Assets.InfiniteArena;
using Assets.InfiniteArena.Other;
using PugMod;
using Unity.Entities;
using UnityEngine;

namespace Assets.InfinieArena
{
    public class InfinieArenaMod : IMod
    {
        private static CustomScenesDataTable sceneData;
        private static CustomScenesDataTable SceneData
        {
            get
            {
                if (sceneData == null)
                {
                    sceneData = Resources.Load<CustomScenesDataTable>("Scenes/CustomScenesDataTable");
                }
                return sceneData;
            }
        }
        private static ModConfig config;
        internal static ModConfig Config => config ??= new();
        public void EarlyInit()
        {
            ArenaRecord.Load();
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
            if (Input.GetKeyDown(KeyCode.K))
            {
                EntityManager entityManager = API.Server.World.EntityManager;
                var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PugDatabase.DatabaseBankCD>());
                BlobAssetReference<PugDatabase.PugDatabaseBank> blob = query.GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob;
                EntityUtility.CreateEntity(API.Server.World, Manager.main.player.WorldPosition.RoundToInt2().ToFloat3(), ObjectID.EventTerminal, 1, blob, 1);
            }
        }
    }
}
