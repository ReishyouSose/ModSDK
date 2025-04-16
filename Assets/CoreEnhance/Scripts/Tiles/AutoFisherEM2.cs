using Assets.CoreEnhance.Scripts.Helpers;
using Assets.CoreEnhance.Scripts.Items;
using Pug.Sprite;
using System.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherEM2 : Chest
    {
        public SpriteObject MainSprite => spriteObjects[0];
        public SpriteObject HookSprite => spriteObjects[1];

        public SpriteObject Light => spriteObjects[2];
        private AutoFisherState oldState;
        private int oldRod;
        private Vector3 WaterPos;
        private int dirIndex;
        private readonly static Vector3[] MainPos = new Vector3[4]
        {
            new(0, 1.625f, 0.125f),
            new(0, 1.1875f, 0.1875f),
            new(0, 0.5f, -0.5f),
            new(0, 1.1875f, 0.1875f),
        };
        private readonly static Vector3[] HookPos = new Vector3[4]
        {
            new(0, 0.875f, -0.6875f),
            new(0, 1.25f, 0.1875f),
            new(0, 0.25f, 1.25f),
            new(0, 1.25f, 0.1875f),
        };
        protected override void Awake()
        {
            //HookSprite.gameObject.SetActive(false);
            GetComponent<RotationTile>().ExtraRotEvent += ExtraRot;
            base.Awake();
        }

        private void ExtraRot(float3 dir, int index)
        {
            WaterPos = EntityUtility.GetComponentData<LocalTransform>(entity, world).Position + dir;
            dirIndex = index;
            var pos = MainPos[index];
            if (pos != Vector3.zero)
                MainSprite.transform.localPosition = MainPos[index];
            pos = HookPos[index];
            if (pos != Vector3.zero)
                HookSprite.transform.localPosition = HookPos[index];
            spriteObjects[5].gameObject.SetActive(index == 0);
            if (index != 2)
            {
                MainSprite.transform.rotation = new(0, 0, 0, 1);
                HookSprite.transform.rotation = new(0, 0, 0, 1);
            }
        }

        public override void ManagedLateUpdate()
        {
            base.ManagedLateUpdate();
            UpdateVisiual();
        }
        private void UpdateVisiual()
        {
            var info = EntityUtility.GetComponentData<AutoFisherCD>(entity, world);
            var state = info.state;
            Light.gameObject.SetActive(EntityUtility.GetComponentData<ElectricityCD>(entity, world)
                .hasEnoughElectricityToPowerStuff);
            var rod = info.rodLevel;
            /*if (oldRod != rod)
            {
                oldRod = rod;
                if (rod > 0)
                {
                    HookSprite.gameObject.SetActive(true);
                }
                else
                {
                    StopAndReset(dirIndex);
                    HookSprite.gameObject.SetActive(false);
                }
            }*/
            if (oldState != state)
            {
                oldState = state;
                switch (state)
                {
                    case AutoFisherState.Start:
                        StartCoroutine(StartAnim());
                        break;
                    case AutoFisherState.End:
                        StartCoroutine(EndAnim());
                        break;
                    case AutoFisherState.Idle:
                    case AutoFisherState.Catching:
                        break;
                }
            }
        }
        private IEnumerator StartAnim()
        {
            MainSprite.PlayAnimationByIndex(0, dirIndex);
            HookSprite.PlayAnimationByIndex(0, dirIndex);
            yield return new WaitForSeconds(0.4f);
            Manager.effects.PlayPuff(PuffID.SmallWaterSplash, WaterPos, 1);
            yield return new WaitForSeconds(0.3f);
            StopAndReset(dirIndex + 4);
            yield return null;
            yield break;
        }
        private IEnumerator EndAnim()
        {
            MainSprite.PlayAnimationByIndex(0, dirIndex + 4);
            HookSprite.PlayAnimationByIndex(0, dirIndex + 4);
            yield return new WaitForSeconds(0.4f);
            Manager.effects.PlayPuff(PuffID.SmallWaterSplash, WaterPos, 1);
            yield return new WaitForSeconds(0.3f);
            StopAndReset(dirIndex);
            yield return null;
            yield break;
        }
        private void StopAndReset(int index)
        {
            MainSprite.StopAnimation();
            MainSprite.SetVariantByIndex(index);
            MainSprite.ApplyVisualChange();
            HookSprite.StopAnimation();
            HookSprite.SetVariantByIndex(index);
            HookSprite.ApplyVisualChange();
        }
    }
}
