using Interaction;
using Pug.UnityExtensions;
using PugConversion;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Components
{
    [RequireComponent(typeof(ObjectAuthoring))]
    public class ModInteractableAuthoring : MonoBehaviour
    {
    }
    public class ModInteractableConverter : PugPostConverter
    {
        public override void PostConvert(GameObject authoring)
        {
            Entity entity = GetEntity(authoring);
            InteractableObject[] array = null;
            ObjectAuthoring component2 = authoring.GetComponent<ObjectAuthoring>();
            Debug.Log($"Register Interactable {component2.objectName}");
            if (component2 != null && component2.graphicalPrefab != null)
            {
                array = component2.graphicalPrefab.GetComponentsInChildren<InteractableObject>(true);
            }
            ObjectInfo info = component2.ObjectInfo;
            InteractableObject interactableObject = array[0];
            FourDirectionFloat2 fourDirectionFloat = new();
            for (int i = 0; i < 4; i++)
            {
                int2 direction = Direction.allFourClockwise[i].f3.RoundToInt2();
                DirectionCD.RotateTransform(interactableObject.transform.rotation, interactableObject.transform.position, DirectionBasedOnVariationCD.GetVariationFromDirection(direction, false), info.prefabCornerOffset.ToInt2(), info.prefabTileSize.ToInt2(), out quaternion quaternion, out float3 x);
                fourDirectionFloat.SetDataInDirection(Direction.allFourClockwise[i].id, x.ToFloat2());
            }
            BlobAssetReference<InteractablePointOffsetsData> interactablePointOffsetsData = CreateInteractablePointsBlob(info.prefabInfos[0].prefab.gameObject, interactableObject, fourDirectionFloat, info);
            BlobAssetStore.TryAdd(ref interactablePointOffsetsData);
            InteractableData data = new()
            {
                interactRadiusSqr = interactableObject.radius * interactableObject.radius,
                requiredFactionToInteract = interactableObject.requiredFactionToInteract,
                allowToUseOnlyWhenClaimed = interactableObject.allowToUseOnlyWhenClaimed,
                weightMultiplier = interactableObject.weightMultiplier,
                directionOffset = fourDirectionFloat,
                ignorePlayerDirection = interactableObject.ignorePlayerDirection
            };
            BlobAssetReference<InteractableData> interactableData = CreateAndAddBlobAsset(data, 128);
            EntityManager.AddComponentData(entity, new InteractableCD
            {
                interactablePointOffsetsData = interactablePointOffsetsData,
                interactableData = interactableData
            });
            EntityManager.AddComponentData(entity, default(InteractableObjectReferenceCD));
        }

        // Token: 0x06008919 RID: 35097 RVA: 0x00232FBC File Offset: 0x002311BC
        private BlobAssetReference<T> CreateAndAddBlobAsset<T>(T data, int chunkSize = 65536) where T : unmanaged
        {
            BlobBuilder val = new(Allocator.Temp, chunkSize);
            try
            {
                val.ConstructRoot<T>() = data;
                BlobAssetReference<T> result = val.CreateBlobAssetReference<T>(Allocator.Persistent);
                BlobAssetStore.TryAdd(ref result);
                return result;
            }
            finally
            {
                val.Dispose();
            }
        }

        // Token: 0x0600891A RID: 35098 RVA: 0x00233028 File Offset: 0x00231228
        private BlobAssetReference<InteractablePointOffsetsData> CreateInteractablePointsBlob(GameObject visualPrefab, InteractableObject interactableObject, FourDirectionFloat2 interactionPointOffset, ObjectInfo objectInfo)
        {
            OffsetFromEntityDirectionOrVariation[] offsetComp = Array.Empty<OffsetFromEntityDirectionOrVariation>();
            BlobBuilder blobBuilder = new(Allocator.Temp, 512);
            ref InteractablePointOffsetsData ptr = ref blobBuilder.ConstructRoot<InteractablePointOffsetsData>();
            int count = interactableObject.interactingPoints.Count;
            BlobBuilderArray<InteractablePointsOffsetsInDirection> blobBuilderArray = blobBuilder.Allocate(ref ptr.pointOffsets, Direction.allFourClockwise.Length);
            for (int i = 0; i < Direction.allFourClockwise.Length; i++)
            {
                AddInteractablePointsByDirection(ref blobBuilder, ref blobBuilderArray, count, interactableObject, i, offsetComp, interactionPointOffset, objectInfo);
            }
            BlobAssetReference<InteractablePointOffsetsData> result = blobBuilder.CreateBlobAssetReference<InteractablePointOffsetsData>(Allocator.Persistent);
            blobBuilder.Dispose();
            return result;
        }

        // Token: 0x0600891B RID: 35099 RVA: 0x002330B4 File Offset: 0x002312B4
        private void AddInteractablePointsByDirection(ref BlobBuilder blobBuilder, ref BlobBuilderArray<InteractablePointsOffsetsInDirection> pointsByDirection, int validPoints, InteractableObject interactableObject, int directionIndex, OffsetFromEntityDirectionOrVariation[] offsetComp, FourDirectionFloat2 interactionPointOffset, ObjectInfo objectInfo)
        {
            BlobBuilderArray<float3> val = blobBuilder.Allocate(ref pointsByDirection[directionIndex].values, (validPoints <= 0) ? 1 : validPoints);
            Direction direction = Direction.allFourClockwise[directionIndex];
            int2 direction2 = direction.f3.RoundToInt2();
            if (validPoints > 0)
            {
                for (int i = 0; i < interactableObject.interactingPoints.Count; i++)
                {
                    Transform transform = interactableObject.interactingPoints[i];
                    DirectionCD.RotateTransform(transform.rotation, transform.position, DirectionBasedOnVariationCD.GetVariationFromDirection(direction2), objectInfo.prefabCornerOffset.ToInt2(), objectInfo.prefabTileSize.ToInt2(), out var _, out var newTranslation);
                    val[i] = newTranslation - interactionPointOffset.GetDataInDirection(direction.id, float2.zero).ToFloat3();
                }
            }
        }
    }
}
