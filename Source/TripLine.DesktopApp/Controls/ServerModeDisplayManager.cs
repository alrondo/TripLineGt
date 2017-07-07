
namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TLine.DpSystem.Service.Client;
    using System.Threading.Tasks;

    public class ServerModeDisplayManager
    {
        private readonly ServerClient _client;

        public TextBlock ServerModeTextBlock { get; private set; }

        private bool IsEmulated { get; set; }

        public ServerModeDisplayManager(ServerClient client)
        {
            _client = client;
            ServerModeTextBlock = new TextBlock() {FontSize = 20, VerticalAlignment = VerticalAlignment.Center,
                                                   HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(12, 0, 0, 0) };
        }

        public void GetServerMode()
        {
            if (IsEmulated)
                return;

            var serverIsEmulating = _client.GetServerMode();
            if (serverIsEmulating)
            {
                IsEmulated = true;
                ServerModeTextBlock.Text = (serverIsEmulating) ? "Server is in emulation" : "";
                Notify.NotifySystem.Default.ShowError("Server is in emulation");
            }
        }

        public void Reset()
        {
            IsEmulated = false;
            ServerModeTextBlock.Text = "";
        }
    }
}