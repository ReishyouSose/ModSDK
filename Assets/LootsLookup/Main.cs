using PugMod;
using System.Linq;
using UnityEngine;

namespace Assets.LootsLookup
{
    public class Main : IMod
    {
        public void EarlyInit()
        {

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
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.K))
            {
                var player = Manager.main.player;
                if (player == null)
                    return;
                var id = player.GetEquippedSlot().objectData.objectID;
                Debug.Log("[Check Loot] " + id);
                foreach (var lt in Manager.mod.LootTable)
                {
                    foreach (var dr in lt.lootInfos)
                    {
                        if (dr.objectID == id)
                        {
                            Debug.Log(lt.id);
                            break;
                        }
                    }
                }
            }
        }
    }
}