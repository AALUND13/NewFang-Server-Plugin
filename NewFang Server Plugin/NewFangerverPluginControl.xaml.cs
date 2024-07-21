using NewFangServerPlugin.Configs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewFangServerPlugin {
    public partial class NewFangServerPluginControl : UserControl {

        private NewFangServerPlugin Plugin { get; }

        private NewFangServerPluginControl() {
            InitializeComponent();
        }

        public NewFangServerPluginControl(NewFangServerPlugin plugin) : this() {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e) {
            Plugin.Save();
        }


        private void UIElement_OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key != Key.Delete)
                return;

            var list = (DataGrid)sender;
            list.SelectedItems.Cast<ConnectedWebhookURL>().ToList().ForEach(item => Plugin.Config.ConnectedWebhookURLs.Remove(item));
        }

        private void AddWebhookURL_OnClick(object sender, RoutedEventArgs e) {
            Plugin.Config.ConnectedWebhookURLs.Add(new ConnectedWebhookURL());
        }

        private void RemoveWebhookURL_OnClick(object sender, RoutedEventArgs e) {
            WebhookUrlsGrid.SelectedItems.Cast<ConnectedWebhookURL>().ToList().ForEach(item => Plugin.Config.ConnectedWebhookURLs.Remove(item));
        }
    }
}
