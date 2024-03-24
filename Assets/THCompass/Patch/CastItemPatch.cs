using Assets.THCompass.Component;
using Assets.THCompass.Helper;
using HarmonyLib;
using PlayerState;
using Unity.Mathematics;
using static Assets.THCompass.Compasses.CompassData;
using static Assets.THCompass.Compasses.CompassLootTable;
using static Assets.THCompass.THCompassMain;

namespace Assets.THCompass.Patch
{
    [HarmonyPatch]
    public class CastItemPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Casting), "StartCastingItem")]
        private static bool Start(Casting __instance)
        {
            ObjectDataCD item = __instance.GetField<ObjectDataCD>("objectData");
            if (PugDatabase.HasComponent<DropFromBossCD>(item))
            {
                __instance.pc.PlayAnimationTrigger(AnimID.startTeleport);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Casting), "FinishCastingItem")]
        private static bool Finish(Casting __instance)
        {
            ObjectDataCD item = __instance.GetField<ObjectDataCD>("objectData");
            //bool used = __instance.GetField<bool>("itemIsInProcessOfBeingUsed");
            //used = true;
            PlayerController player = __instance.pc;
            /*if (Manager.saves.IsCreativeModeWorld())
            {
                if (PugDatabase.HasComponent<RoomSpawnerCD>(item))
                {
                    //THCompassMain.roomSpawnSystem.SpawnRoom(Manager.main.player.WorldPosition,
                    //THCompassMain.SceneData.scenes.FindIndex(x => x.sceneName == "BrokenCore1"**"AncientMaze"**));
                    player.sWalk.EnterState();
                    //player.ConsumeEntityInSlot(player.GetEquippedSlot(), 1);
                    return false;
                }
            }
            else */
            if (item.TryGetComponent(out DropFromBossCD info))
            {
                player.sWalk.EnterState();
                bool ten = item.amount >= 10;
                if (TileHelper.FindNearestSpace(7, ten, ten && (info.bossID is BossID.Atlantis or BossID.Octopus),
                    player.world.EntityManager, player.WorldPosition, out float3 pos, out int dir))
                {
                    compassLootSystem.CompassLoot(info.bossID, ten, pos, dir);
                    player.ConsumeItem(ten ? 10 : 1);
                }
                return false;
            }
            return true;
        }
    }
}
