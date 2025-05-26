using Pug.UnityExtensions;
using PugMod;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.MyTest
{
    public class MyTestMod : IMod
    {
        public void EarlyInit()
        {
            API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
        }

        private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
        {
            if (authoringData.TryGetComponent<EntityMonoBehaviourData>(out var data))
            {
                if (data.ObjectInfo.objectID == ObjectID.EventTerminal)
                {
                    Debug.Log(data.objectInfo.variation);
                    int count = authoringData.GetComponentCount();
                    for (int i = 0; i < count; i++)
                    {
                        var comp = authoringData.GetComponentAtIndex(i);
                        Debug.Log(comp);
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
            if (Input.GetKeyDown(KeyCode.K))
            {
                GetEntitiesAt(Manager.main.player.WorldPosition.RoundToInt2(), out var entities);
                Debug.Log(string.Join("\n", entities));
            }
        }
        private static void GetEntitiesAt(int2 position, out List<ObjectID> entities)
        {
            entities = new List<ObjectID>();
            var entityManager = API.Client.World.EntityManager;
            var queryDesc = new EntityQueryDesc
            {
                All = new[]
                    { ComponentType.ReadOnly<ObjectDataCD>(), ComponentType.ReadOnly<LocalTransform>() },
                None = new[] { ComponentType.ReadOnly<PlayerGhost>() }
            };
            var query = entityManager.CreateEntityQuery(queryDesc);
            var array = query.ToEntityArray(Allocator.Temp);
            foreach (var entity2 in array)
            {
                var transform = entityManager.GetComponentData<LocalTransform>(entity2);
                var objData = entityManager.GetComponentData<ObjectDataCD>(entity2);
                var actualPosition = transform.Position.RoundToInt2();
                if (position.Equals(actualPosition))
                    entities.Add(objData.objectID);
            }

            array.Dispose();
        }
    }
    /*        private static void LoadConfig()
        {
            if (!API.ConfigFilesystem.DirectoryExists(ConfigFolder))
                API.ConfigFilesystem.CreateDirectory(ConfigFolder);

            string configFile = Path.Combine(ConfigFolder, EventsFileName);

            if (!API.ConfigFilesystem.FileExists(configFile))
            {
                Console.WriteLine("No config file found, creating new one");
                Instance.Events = Instance.ExampleEvents.ToArray();
                SaveConfig();
                return;
            }

            Console.WriteLine("Found config file, loading...");
            byte[] x = API.ConfigFilesystem.Read(configFile);

            if (x == null)
            {
                Console.WriteLine("Empty config file found, creating new one");
                SaveConfig();
                return;
            }

            try
            {
                instance.Events = JsonConvert.DeserializeObject<TwitchEventData[]>(Encoding.UTF8.GetString(x));

                Console.WriteLine($"Loaded config with {instance.Events.Length} events");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load config: {e}");
            }
        }

        private static void SaveConfig()
        {
            if (!API.ConfigFilesystem.DirectoryExists(ConfigFolder))
                API.ConfigFilesystem.CreateDirectory(ConfigFolder);

            string configFile = Path.Combine(ConfigFolder, EventsFileName);

            Console.WriteLine($"Saving config to {configFile}");
            try
            {
                string savedata = JsonConvert.SerializeObject(instance.Events, SerializerSettings);

                API.ConfigFilesystem.Write(configFile, Encoding.UTF8.GetBytes(savedata));

                Console.WriteLine($"Saved config with {instance.Events.Length} events.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to save config: {e}");
            }
        }*
    
     internal static void Setup()
        {
            var configver =  API.Config.TryGet("FLXSNX", "flxsnx", "ConfigVersion", out int configVersion);
            if (!configver)
            {
                API.Config.Register("FLXSNX", "flxsnx", "Config version", "Version", ConfigVersion);
            }

            API.Config.TryGet("FLXSNX", "flxsnx", "Open", out string key);
            API.Config.TryGet("FLXSNX", "systems", "PauseWater", out bool pauseWater);
            
            if (configVersion < ConfigVersion)
            {
                API.Config.Register("FLXSNX", "flxsnx", "Key to open FLXSNX menu", "Open", KeyCode.F11.ToString());
                API.Config.Register("FLXSNX", "systems", "Pause water setting", "PauseWater", false);
                
            }
            EnableKey = Enum.Parse<KeyCode>(key, true);
        }*/
}
