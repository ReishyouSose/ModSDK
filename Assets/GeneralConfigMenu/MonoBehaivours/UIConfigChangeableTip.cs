using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GeneralConfigMenu.MonoBehaivours
{
    [RequireComponent(typeof(UIConfigEntry))]
    public class UIConfigChangeableTip :MonoBehaviour
    {
        public SpriteRenderer ViewOnly;
        public SpriteRenderer Client;
        public SpriteRenderer Server;
        public SpriteRenderer Admin;
        public void Awake()
        {
            ViewOnly.gameObject.SetActive(false);
            Client.gameObject.SetActive(false);
            Server.gameObject.SetActive(false);
            Admin.gameObject.SetActive(false);
            SwitchTip();
        }
        public void SwitchTip()
        {
            var config = GetComponent<UIConfigEntry>();
            /*switch (config.Scope.AccessLevel)
            {
            
            }*/
        }
    }
}
