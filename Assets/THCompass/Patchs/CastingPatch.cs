namespace Assets.THCompass.Patchs
{
    /*[HarmonyPatch]
    public class CastingPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Casting), "StartCastingItem")]
        public static bool StartCastingItem(Entity equippedObjectPrefab, ObjectID objectID, DynamicBuffer<AnimationBuffer> animationBuffer, ref AnimationBufferPointer animationBufferPointer, ChangePlayerStateLookup changePlayerStateLookup, NetworkTick currentTick)
        {
            Debug.Log("Start");
            if (objectID.TryGetComponent<DropFromBossCD>(out _))
            {
                Debug.Log("CompassStart");
                PlayerController.PlayAnimationTrigger(-1518581387, currentTick, animationBuffer, ref animationBufferPointer);
                return false;
            }
            else
            {
                Debug.Log("NotCompass");
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Casting), "FinishCastingItem")]
        public static bool FinishCastingItem(StateUpdateAspect stateUpdateAspect, SharedStateUpdateData sharedStateUpdateData, LookupStateUpdateData lookupStateUpdateData)
        {
            stateUpdateAspect.castingStateCD.ValueRW.itemIsInProcessOfBeingUsed = true;
            var item = stateUpdateAspect.equippedObjectCD.ValueRO.containedObject.objectData;
            if (item.TryGetComponent(out DropFromBossCD info))
            {
                Debug.Log("CompassFinish");
                stateUpdateAspect.playerStateCD.ValueRW.SetNextState(PlayerStateEnum.Walk);
                bool ten = item.amount >= 10;
                float3 position = Manager.main.player.WorldPosition.RountToFloat3();
                //if (TileHelper.FindNearestSpace(7, ten, ten && (info.bossID is BossID.Atlantis or BossID.Octopus),
                //    , position, out float3 pos, out int dir))
                //{
                THCompassMain.compassLootSystem.CompassLoot(info.bossID, ten, position, 0);
                //player.ConsumeItem(ten ? 10 : 1);
                DynamicBuffer<InventoryChangeBuffer> val = lookupStateUpdateData.inventoryChangeBuffer[sharedStateUpdateData.inventoryChangeBufferEntity];
                InventoryChangeBuffer inventoryChangeBuffer = default;
                Entity entity = stateUpdateAspect.entity;
                int equippedSlotIndex = stateUpdateAspect.equippedObjectCD.ValueRO.equippedSlotIndex;
                bool dontConsume = false lookupStateUpdateData.godModeLookup.IsComponentEnabled(stateUpdateAspect.entity);
                ContainedObjectsBuffer containedObject = stateUpdateAspect.equippedObjectCD.ValueRO.containedObject;
                inventoryChangeBuffer.inventoryChangeData = Create.ConsumeEntityAt(entity, equippedSlotIndex, 1, destroy: true, dontConsume, position, containedObject.variation);
                val.Add(inventoryChangeBuffer);
                //}
                return false;
            }
            else
            {
                Debug.Log("NotCompass");
            }
            return true;
        }
    }*/
}
