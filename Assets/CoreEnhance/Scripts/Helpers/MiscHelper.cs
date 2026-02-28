using PugMod;
using UnityEngine;
using static CombatText;

namespace Assets.CoreEnhance.Scripts.Helpers
{
    public static class MiscHelper
    {
        public static void NewText(string text)
        {
            Manager.ui.chatWindow.AddInfoText(new string[] { text }, ChatWindow.MessageTextType.Sent);
        }

        public static void NewCombatText(string text, Vector3? position = null, NumberColor color = NumberColor.White)
        {
            var player = Manager.main.player;
            position ??= Manager.ui.isAnyInventoryShowing ? (player.RenderPosition + Vector3.down * 1.15f + Vector3.back * 0.5f) : (player.RenderPosition + Vector3.up * 0.7f);
            SpawnCombatText(text, color, position.Value, false, false, false);
        }
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out _);
        }

        public static ObjectID GetEntityObjectID(this GameObject gameObject, out int variation)
        {
            var entityMonoBehaviorData = gameObject.GetComponent<EntityMonoBehaviourData>();
            var objectAuthoring = gameObject.GetComponent<ObjectAuthoring>();
            variation = 0;
            if (entityMonoBehaviorData != null)
            {
                var info = entityMonoBehaviorData.objectInfo;
                variation = info.variation;
                return info.objectID;
            }
            if (objectAuthoring != null)
            {
                variation = objectAuthoring.variation;
                return API.Authoring.GetObjectID(objectAuthoring.objectName);
            }

            return ObjectID.None;
        }
    }
}
