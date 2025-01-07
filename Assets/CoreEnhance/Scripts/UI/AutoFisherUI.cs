using Assets.CoreEnhance.Scripts.Systems.Misc;
using Assets.CoreEnhance.Scripts.Tiles;
using CoreLib.UserInterface;
using Unity.Entities;
using UnityEngine;

namespace Assets.CoreEnhance.Scripts.UI
{
    public class AutoFisherUI : MonoBehaviour, IModUI
    {
        public GameObject Root => gameObject;

        public bool showWithPlayerInventory => true;

        public bool shouldPlayerCraftingShow => false;
        public static AutoFisherUI Ins { get; private set; }

        public PugText Exp;
        private EntityManager manager;
        private Entity AutoFisher;
        private void Awake()
        {
            Ins = this;
            HideUI();
        }

        public void HideUI()
        {
            Root.SetActive(false);
            AutoFisher = Entity.Null;
        }

        public void ShowUI()
        {
            if (AutoFisher == Entity.Null)
            {
                Debug.Log("Not set auto fisher entity");
                return;
            }
            Root.SetActive(true);
        }
        public void SetAutoFisher(AutoFisherEM em)
        {
            AutoFisher = em.entity;
            manager = em.world.EntityManager;
        }
        private void Update()
        {
            Exp.Render(manager.GetComponentData<ObjectDataCD>(AutoFisher).amount.ToString());
        }
        public void ReceiveExp()
        {
            AutoFisherClient.ReceiveExp(AutoFisher, Manager.main.player.entity);
        }
    }
}
