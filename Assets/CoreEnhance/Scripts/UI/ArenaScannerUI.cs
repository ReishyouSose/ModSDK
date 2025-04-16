using Assets.CoreEnhance.Scripts.Systems.Misc;
using CoreLib.UserInterface;
using PugMod;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class ArenaScannerUI : MonoBehaviour, IModUI
    {
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        public SpriteRenderer Pointer;
        public Transform Ring;
        private bool init;
        private float timer;
        public List<SpriteRenderer> pointers;
        private static ArenaScannerUI ins;
        private static ObjectID scanner;
        private void Awake()
        {
            HideUI();
            ins = this;
            Pointer.gameObject.SetActive(false);
            pointers = new();
        }
        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
            if (init)
                return;
            init = true;
            scanner = API.Authoring.GetObjectID("CoreEnhance:ArenaScanner");
        }
        private void Update()
        {
            var p = Manager.main.player;
            if (p == null || Manager.ui.isAnyInventoryShowing)
            {
                HideUI();
                return;
            }
            if (timer < 0.1f)
            {
                timer += API.Client.World.Time.DeltaTime;
                return;
            }
            timer = 0;
            if (ArenaScannerClient.Ins == null)
                return;
            var arenas = ArenaScannerClient.Ins.arenas;
            foreach (var pointer in pointers)
            {
                pointer.gameObject.SetActive(false);
            }
            var player = p.WorldPosition.RoundToInt2();
            int i = 0;
            foreach (var arena in arenas)
            {
                var pos = arena;
                var offset = pos - player;
                var dis = math.distancesq(offset.x, offset.y);
                var dir = math.normalizesafe(pos - player, int2.zero) * 6;
                if (pointers.Count == i)
                {
                    pointers.Add(Instantiate(Pointer, Ring));
                }
                var pointer = pointers[i];
                pointer.gameObject.SetActive(true);
                pointer.transform.localPosition = new(dir.x, dir.y, 0);
                i++;
            }
        }
        public static void CheckScanner(PlayerController player)
        {
            var lockState = API.Client.World.EntityManager.GetBuffer<LockedObjectsBuffer>(player.entity);
            var containers = API.Client.World.EntityManager.GetBuffer<ContainedObjectsBuffer>(player.entity);
            bool hasScanner = false;
            for (int i = 0; i < containers.Length; i++)
            {
                if (containers[i].objectID == scanner && lockState[i].Value)
                {
                    hasScanner = true;
                    break;
                }
            }
            if (!hasScanner)
            {
                if (ins.gameObject.activeInHierarchy)
                {
                    ArenaScannerClient.SiwtchFunction(false);
                    ins.HideUI();
                }
            }
            else
            {
                if (!ins.gameObject.activeInHierarchy && !Manager.ui.isAnyInventoryShowing)
                {
                    ArenaScannerClient.SiwtchFunction(true);
                    ins.ShowUI();
                }
            }
        }
    }
}
