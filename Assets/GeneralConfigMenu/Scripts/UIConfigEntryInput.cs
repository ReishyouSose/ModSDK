namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigEntryInput : UIConfigEntry
    {
        private InputBox server, client;
        public override void SetChanger()
        {
            string value = ConfigEntry.GetSerializedValue();
            server = ServerChanger.GetComponent<InputBox>();
            server.SetInputText(value);
            server.onInputFieldDone.AddListener(OnTextChanged_Server);
            server.AllowInput += CheckAdmin;

            client = ClientChanger.GetComponent<InputBox>();
            client.SetInputText(value);
            client.onInputFieldDone.AddListener(OnTextChanged_Client);
        }
        private void OnTextChanged_Server()
        {
            string text = server.GetInputText();
            if (ValueEquals(text))
                return;
            SetAndSendChange(text);
            server.SetInputText(ConfigEntry.GetSerializedValue());
            if (AutoStoC)
                TryServerToClient();
        }
        private void OnTextChanged_Client()
        {
            string text = client.GetInputText();
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
