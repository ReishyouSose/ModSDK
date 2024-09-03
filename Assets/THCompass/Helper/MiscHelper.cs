using PugMod;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.THCompass.Helper
{
    public static class MiscHelper
    {
        public static float3 WorldPos(this Entity entity) => EntityUtility.GetComponentData<LocalTransform>(entity, API.Server.World).Position;
        public static int2 Modify(this int2 value, int xFix, int yFix) => new(value.x + xFix, value.y + yFix);
        public static float3 Modify(this float3 value, float xFix, float yFix, float zFix)
        {
            return new(value.x + xFix, value.y + yFix, value.z + zFix);
        }
        public static float3 ToRotationFloat3(this float value)
        {
            return new(MathF.Cos(value), 0, MathF.Sin(value));
        }
        public static float3 RountToFloat3(this Vector3 value) => value.RoundToInt().ToFloat3();
        public static NativeList<T> GetRange<T>(this NativeList<T> list, int start, int end) where T : unmanaged
        {
            NativeList<T> list2 = new();
            for (int i = start; i < end; i++)
            {
                if (i >= list.Length) break;
                list2.Add(list[i]);
            }
            return list2;
        }
    }
}
