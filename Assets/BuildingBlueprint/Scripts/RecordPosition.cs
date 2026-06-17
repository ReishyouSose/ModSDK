using Pug.UnityExtensions;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class RecordPosition : MonoBehaviour
    {
        public float3 Position;
        public static implicit operator float3(RecordPosition r) => r.Position;
        public static implicit operator int2(RecordPosition r) => r.Position.RoundToInt2();
    }
}
