using Assets.GeneralConfigMenu.RUIFramework;
using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIConfigEntryBool : UIConfigEntry
    {
        private RUIButton server, client;
        private bool defaultV;
        public override void SetChanger()
        {
            defaultV = bool.Parse(DefaultValue);
            var entry = ConfigEntry as ConfigEntry<bool>;
            bool value = entry.Value;

            server = ServerChanger.GetComponent<RUIButton>();
            server.ToggleAtFirst = value;
            server.OnValueChange += OnValueChange_Server;
            server.AllowEvent += CheckAdmin;

            client = ClientChanger.GetComponent<RUIButton>();
            client.ToggleAtFirst = value;
            client.OnValueChange += OnValueChange_Client;
        }
        private void OnValueChange_Server(RUIButton button)
        {
            SetAndSendChange(button.IsToggle.ToString());
            if (AutoStoC)
                TryServerToClient();
        }
        private void OnValueChange_Client(RUIButton button)
        {
            SetClient(button.IsToggle.ToString());
            if (AutoCtoS)
                TryClientToServer();
        }
        protected override void ServerToClient()
        {
            client.SetState(server.IsToggle, false, true);
            SetClient(server.IsToggle.ToString());
        }
        protected override void ClientToServer()
        {
            server.SetState(client.IsToggle, false, true);
            SetAndSendChange(client.IsToggle.ToString());
        }
        public override void ReceiveSync(string value)
        {
            if (bool.TryParse(value, out var result))
            {
                server.SetState(result, false, true);
            }
        }
        public override void ServerReset()
        {
            server.SetState(defaultV, false, true);
            SetAndSendChange(DefaultValue);
        }
        public override void ClientReset()
        {
            client.SetState(defaultV, false, true);
            SetClient(DefaultValue);
        }
        public override bool ValueEquals(string value)
        {
            if (bool.TryParse(value, out var result))
            {
                return result == ((ConfigEntry<bool>)ConfigEntry).Value;
            }
            return base.ValueEquals(value);
        }
        public override bool ServerEqualsClient() => server.IsToggle == client.IsToggle;
    }
}
