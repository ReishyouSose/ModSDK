using Assets.GeneralConfigMenu.RUIFramework;
using CoreLib.Data.Configuration;
using System;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigAccessLevel : MonoBehaviour
    {
        public SpriteRenderer ViewOnlyIcon, ClientIcon, ServerIcon, AdminIcon;
        public void SetLevel(ConfigAccessLevel level)
        {
            bool viewOnly, client, server, admin;
            viewOnly = client = server = admin = false;
            switch (level)
            {
                case ConfigAccessLevel.ViewOnly:
                    viewOnly = true;
                    break;
                case ConfigAccessLevel.Client:
                    client = true;
                    break;
                case ConfigAccessLevel.Server:
                    server = true;
                    break;
                case ConfigAccessLevel.Admin:
                    admin = true;
                    break;
            }
            ViewOnlyIcon.gameObject.SetActive(viewOnly);
            ClientIcon.gameObject.SetActive(client);
            ServerIcon.gameObject.SetActive(server);
            AdminIcon.gameObject.SetActive(admin);
        }
    }
}
