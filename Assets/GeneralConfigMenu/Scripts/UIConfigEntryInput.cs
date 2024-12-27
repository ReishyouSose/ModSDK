using Assets.GeneralConfigMenu.UIByLimoka;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigEntryInput : UIConfigEntry
    {
        private BetterInputField server, client;
        public override void SetChanger()
        {
            string value = ConfigEntry.GetSerializedValue();
            server = ServerChanger.GetComponent<BetterInputField>();
            server.SetInputText(value);
            server.onTextChanged += OnTextChanged_Server;
            server.AllowInput += CheckAdmin;

            client = ClientChanger.GetComponent<BetterInputField>();
            client.SetInputText(value);
            client.onTextChanged += OnTextChanged_Client;
        }
        private void OnTextChanged_Server(GameObject go, string text)
        {
            if (ValueEquals(text))
                return;
            SetAndSendChange(text);
            server.SetInputText(ConfigEntry.GetSerializedValue());
            if (AutoStoC)
                TryServerToClient();
        }
        private void OnTextChanged_Client(GameObject go, string text)
        {
            if (ValueEquals(text))
                return;
            SetClient(text);
            client.SetInputText(ConfigEntry.GetSerializedValue());
            if (AutoCtoS)
                TryClientToServer();
        }
        protected override void ServerToClient()
        {
            var value = server.GetInputText();
            client.SetInputText(value);
            SetClient(value);
        }
        protected override void ClientToServer()
        {
            var value = client.GetInputText();
            server.SetInputText(value);
            SetAndSendChange(value);
        }
        public override void ReceiveSync(string value)
        {
            server.SetInputText(value);
        }
        public override void ServerReset()
        {
            server.SetInputText(DefaultValue);
            SetAndSendChange(DefaultValue);
        }
        public override void ClientReset()
        {
            client.SetInputText(DefaultValue);
            SetClient(DefaultValue);
        }
        public override bool ServerEqualsClient() => server.GetInputText() == client.GetInputText();
    }
}
