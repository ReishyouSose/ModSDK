using Unity.Mathematics;
using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class RecordPosition : MonoBehaviour
    {
        public int2 Position;
        public static implicit operator int2(RecordPosition r) => r.Position;
    }
}
