using Assets.GeneralConfigMenu.RUIFramework;
using Assets.GeneralConfigMenu.RUIFramework.Extend;
using CoreLib.Data.Configuration;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigEntryList : UIConfigEntry
    {
        private PugText server, client;
        private RUIExpand ServerExpand, ClientExpand;
        public override void SetChanger()
        {
            var value = ConfigEntry.GetSerializedValue();
            server = ServerChanger.GetComponentInChildren<PugText>();
            server.Render(value);
            client = ClientChanger.GetComponentInChildren<PugText>();
            client.Render(value);
        }
        public void LoadExpand(string[] accepts, RUIElement lockUIE)
        {
            ServerExpand = ServerChanger.GetComponent<RUIExpand>();
            ServerExpand.AllowEvent += CheckAdmin;
            LoadExpand(ServerExpand, accepts, lockUIE, InnerLeftDown_Server);
            LoadExpand(ClientExpand = ClientChanger.GetComponent<RUIExpand>(), accepts, lockUIE, InnerLeftDown_Client);
        }
        private void LoadExpand(RUIExpand expand, string[] accepts, RUIElement lockUIE, Action<GameObject> InnerLeftDown)
        {
            var template = expand.InnerTemplate;
            expand.LockElement = lockUIE;
            template.SetActive(false);
            var label = expand.GetComponentInChildren<PugText>();
            expand.ExpandView.Reload((par, view) =>
            {
                foreach (var accept in accepts)
                {
                    var inner = Instantiate(template, view);
                    inner.SetActive(true);
                    RUIText text = inner.GetComponent<RUIText>();
                    text.Text.Render(accept);
                    text.NeedHoverColor();
                    text.AddEvent(RMouseEventType.LeftDown, InnerLeftDown);
                    par.AddChild(inner);
                }
            }, false);
        }
        public static bool TryExtractAcceptableValues(ConfigEntryBase entry, out string[] accepts)
        {
            AcceptableValueBase accept = entry.Description.AcceptableValues;
            accepts = null;
            if (accept == null)
            {
                return false;
            }
            string description = accept.ToDescriptionString();
            // 匹配 "# Acceptable values: " 后的所有值
            string pattern = @"# Acceptable values:\s*(.+)";
            Match match = Regex.Match(description, pattern);
            if (match.Success)
            {
                string valuesPart = match.Groups[1].Value;
                accepts = valuesPart.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            return false;
        }
        private void InnerLeftDown_Server(GameObject go)
        {
            ServerExpand.SetExpand(false);
            var value = go.GetComponent<RUIText>().Text.displayedTextString;
            if (ValueEquals(value))
                return;
            server.Render(value);
            SetAndSendChange(value);
            if (AutoStoC)
                TryServerToClient();
        }
        private void InnerLeftDown_Client(GameObject go)
        {
            ClientExpand.SetExpand(false);
            var value = go.GetComponent<RUIText>().Text.displayedTextString;
            if (ValueEquals(value))
                return;
            client.Render(value);
            SetClient(value);
            if (AutoCtoS)
                TryClientToServer();
        }
        protected override void ServerToClient()
        {
            var value = server.displayedTextString;
            client.Render(value);
            SetClient(value);
        }
        protected override void ClientToServer()
        {
            var value = client.displayedTextString;
            server.Render(value);
            SetAndSendChange(value);
        }
        public override void ReceiveSync(string value)
        {
            server.Render(value);
        }
        public override void ServerReset()
        {
            server.Render(DefaultValue);
            SetAndSendChange(DefaultValue);
        }
        public override void ClientReset()
        {
            client.Render(DefaultValue);
            SetClient(DefaultValue);
        }
        public override bool ServerEqualsClient() => server.displayedTextString == client.displayedTextString;
    }
}
