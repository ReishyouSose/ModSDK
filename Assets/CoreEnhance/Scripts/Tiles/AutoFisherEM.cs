using Assets.CoreEnhance.Scripts.Items;
using CoreLib.Util.Extensions;
using Pug.Sprite;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherEM : Chest
    {
        public SpriteObject MainSprite { get; private set; }
        public SpriteObject HookSprite { get; private set; }

        public SpriteObject Light { get; private set; }
        private const string AutoFisher = "AutoFisher_";
        private const string Hook = "Hook_";
        private const string Front = "Front_";
        private const string Left = "Left_";
        private const string Back = "Back_";
        private const string Right = "Right_";
        private const string Start = "Start";
        private const string End = "End";
        private AutoFisherState oldState;
        private int oldRod;
        private Vector3 WaterPos;
        private int dirIndex;
        private readonly static int Main_Front_Start = SpriteAsset.StringToHash(AutoFisher + Front + Start);
        private readonly static int Main_Left_Start = SpriteAsset.StringToHash(AutoFisher + Left + Start);
        private readonly static int Main_Back_Start = SpriteAsset.StringToHash(AutoFisher + Back + Start);
        private readonly static int Main_Right_Start = SpriteAsset.StringToHash(AutoFisher + Right + Start);
        private readonly static int Main_Front_End = SpriteAsset.StringToHash(AutoFisher + Front + End);
        private readonly static int Main_Left_End = SpriteAsset.StringToHash(AutoFisher + Left + End);
        private readonly static int Main_Back_End = SpriteAsset.StringToHash(AutoFisher + Back + End);
        private readonly static int Main_Right_End = SpriteAsset.StringToHash(AutoFisher + End + End);

        private readonly static int Hook_Front_Start = SpriteAsset.StringToHash(AutoFisher + Hook + Front + Start);
        private readonly static int Hook_Left_Start = SpriteAsset.StringToHash(AutoFisher + Hook + Left + Start);
        private readonly static int Hook_Back_Start = SpriteAsset.StringToHash(AutoFisher + Hook + Back + Start);
        private readonly static int Hook_Right_Start = SpriteAsset.StringToHash(AutoFisher + Hook + Right + Start);
        private readonly static int Hook_Front_End = SpriteAsset.StringToHash(AutoFisher + Hook + Front + End);
        private readonly static int Hook_Left_End = SpriteAsset.StringToHash(AutoFisher + Hook + Left + End);
        private readonly static int Hook_Back_End = SpriteAsset.StringToHash(AutoFisher + Hook + Back + End);
        private readonly static int Hook_Right_End = SpriteAsset.StringToHash(AutoFisher + Hook + End + End);
        protected override void Awake()
        {
            MainSprite = spriteObjects[0];
            HookSprite = spriteObjects[1];
            Light = spriteObjects[2];
            GetComponent<RotationTile>().ExtraRotEvent += ExtraRot;
            base.Awake();
        }

        private void ExtraRot(float3 dir, int index)
        {
            WaterPos = center + dir.ToVector3() * 1.0625f - Vector3.up * 0.5f;
            dirIndex = index;
            spriteObjects[5].gameObject.SetActive(index == 0);
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
            if (oldRod != rod)
            {
                oldRod = rod;
                if (rod > 0)
                {
                    MainSprite.gameObject.SetActive(true);
                }
                else
                {
                    MainSprite.StopAnimation();
                    MainSprite.SetVariantByIndex(dirIndex);
                    MainSprite.ApplyVisualChange();
                    MainSprite.gameObject.SetActive(false);
                }
            }
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
            MainSprite.PlayAnimation(dirIndex switch
            {
                1 => Main_Left_Start,
                2 => Main_Back_Start,
                3 => Main_Right_Start,
                _ => Main_Front_Start
            });
            HookSprite.PlayAnimation(dirIndex switch
            {
                1 => Hook_Left_Start,
                2 => Hook_Back_Start,
                3 => Hook_Right_Start,
                _ => Hook_Front_Start
            });
            yield return new WaitForSeconds(0.4f);
            Manager.effects.PlayPuff(PuffID.SmallWaterSplash, WaterPos, 1);
            yield return new WaitForSeconds(0.3f);
            MainSprite.StopAnimation();
            MainSprite.SetVariantByIndex(dirIndex + 4);
            MainSprite.ApplyVisualChange();
            HookSprite.StopAnimation();
            HookSprite.SetVariantByIndex(dirIndex + 4);
            HookSprite.ApplyVisualChange();
            yield return null;
            yield break;
        }
        private IEnumerator EndAnim()
        {
            MainSprite.PlayAnimation(dirIndex switch
            {
                1 => Main_Left_End,
                2 => Main_Back_End,
                3 => Main_Right_End,
                _ => Main_Front_End
            });
            HookSprite.PlayAnimation(dirIndex switch
            {
                1 => Hook_Left_End,
                2 => Hook_Back_End,
                3 => Hook_Right_End,
                _ => Hook_Front_End
            });
            yield return new WaitForSeconds(0.4f);
            Manager.effects.PlayPuff(PuffID.SmallWaterSplash, WaterPos, 1);
            yield return new WaitForSeconds(0.3f);
            MainSprite.StopAnimation();
            MainSprite.SetVariantByIndex(dirIndex);
            MainSprite.ApplyVisualChange();
            HookSprite.StopAnimation();
            HookSprite.SetVariantByIndex(dirIndex);
            HookSprite.ApplyVisualChange();
            yield return null;
            yield break;
        }
    }
}
