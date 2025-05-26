using Pug.UnityExtensions;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.Items
{
    [RequireComponent(typeof(PhysicsShapeAuthoring))]
    [RequireComponent(typeof(PlaceableObjectAuthoring))]
    public class ColliderEditor : MonoBehaviour
    {
        public float UpFix;
        public float DownFix;
        public float LeftFix;
        public float RightFix;
        public float BevelRadius;
        private void OnValidate()
        {
            var collider = GetComponent<PhysicsShapeAuthoring>();
            var place = GetComponent<PlaceableObjectAuthoring>();
            var size = place.prefabTileSize;
            float x = size.x, y = size.y;
            Vector2 origin = new((x - 1) / 2, (y - 1) / 2);
            Vector2 offset = new(RightFix - LeftFix, UpFix - DownFix);
            float cy = collider.GetBoxProperties().Center.y;
            var c = (origin + offset / 2).X0Y();
            c.y = cy;
            collider.SetBox(new Unity.Physics.BoxGeometry()
            {
                BevelRadius = BevelRadius,
                Size = new(x + LeftFix + RightFix, 1, y + UpFix + DownFix),
                Center = c,
            });
        }
    }
}
