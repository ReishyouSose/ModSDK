using CoreLib.UserInterface;
using NaughtyAttributes;
using PugMod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class HintContainerUI : MonoBehaviour, IModUI
    {
        [Serializable]
        public class HintIcon
        {
            public int index;
            public ObjectID iconObject;
            public string ModObjectName;
            public SpriteRenderer SR;
        }
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => true;

        public bool shouldPlayerCraftingShow => false;
        internal static HintContainerUI Ins { get; private set; }
        public List<HintIcon> HintIcons = new();
        public int StartIndex;
        public bool AutoPos;

        [ShowIf("AutoPos")]
        [AllowNesting]
        public int MaxWidth = 1;

        [ShowIf("AutoPos")]
        [AllowNesting]
        public float Spread = 1;
        private void Awake()
        {
            Ins = this;
            HideUI();
            int count = HintIcons.Count;
            if (!AutoPos || count == 0)
                return;
            var center = new Vector2(MaxWidth, count / MaxWidth) / 2f;
            for (int i = 0; i < HintIcons.Count; i++)
            {
                var offset = new Vector2(i % MaxWidth, i / MaxWidth) * Spread - center;
                HintIcons[i].SR.gameObject.transform.localPosition = offset;
            }
        }
        private void OnValidate()
        {
            MaxWidth = Math.Max(MaxWidth, 1);
        }
        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        private void Start()
        {
            foreach (var hintIcon in HintIcons)
            {
                if (!string.IsNullOrEmpty(hintIcon.ModObjectName))
                {
                    hintIcon.iconObject = API.Authoring.GetObjectID(hintIcon.ModObjectName);
                }
                if (hintIcon.iconObject == ObjectID.None)
                    continue;
                hintIcon.SR.drawMode = SpriteDrawMode.Simple;
                hintIcon.SR.sprite = PugDatabase.GetObjectInfo(hintIcon.iconObject).icon;
            }
        }
        private void Update()
        {
            var p = Manager.main.player;
            if (p == null)
            {
                return;
            }
            var handler = p.activeInventoryHandler;
            var containers = EntityUtility.GetBuffer<ContainedObjectsBuffer>(handler.inventoryEntity, API.Client.World);
            foreach (var hintIcon in HintIcons)
            {
                hintIcon.SR.gameObject.SetActive(containers[StartIndex + hintIcon.index].objectID != ObjectID.None);
            }
        }
    }
}
