using Assets.CoreEnhance.Scripts.Systems.Misc;
using CoreLib.UserInterface;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class ChestVisiableUI : MonoBehaviour, IModUI
    {
        public GameObject Root => gameObject;
        public Transform BG;
        public SpriteRenderer Template;
        private List<SpriteRenderer> pools;

        public bool showWithPlayerInventory => false;

        public bool shouldPlayerCraftingShow => false;
        private void Awake()
        {
            pools = new();
            Template.gameObject.SetActive(false);
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            gameObject.SetActive(true);
        }
        private void Update()
        {
            var ins = ChestVisiableSystem.Ins;
            if (ins == null)
                return;
            foreach (var sr in pools)
            {
                sr.gameObject.SetActive(false);
            }
            var infos = ins.infos;
            int count = infos.Length;
            if (count == 0)
                return;
            var player = Manager.main.player;
            var wp = Manager.camera.smoothedCameraPosition;
            // Debug.Log(player.SmoothWorldPosition);
            for (int i = 0; i < count; i++)
            {
                if (pools.Count <= i)
                {
                    pools.Add(Instantiate(Template, BG));
                }
                var info = infos[i];
                var sr = pools[i];
                sr.gameObject.SetActive(true);
                var pos = info.pos;
                sr.transform.localPosition = new(pos.x - wp.x, pos.z - wp.z + 0.125f, 0);
            }
        }
    }
}
