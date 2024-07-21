using NewFangServerPlugin.API;
using NewFangServerPlugin.Configs;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.Utils.Webs;
using NLog;
using Sandbox.Game.Gui;
using System.Linq;
using Torch.API.Managers;

namespace NewFangServerPlugin.Handler {
    public class PluginEventHandler {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        public static void OnMessageRecieved(TorchChatMessage msg, ref bool consumed) {
            if(msg.Channel != ChatChannel.Global) return; // Only process global chat messages

            Log.Info($"Message from {msg.Author}: {msg.Message}");
            foreach (ConnectedWebhookURL webhookURL in PluginInstance.Config.ConnectedWebhookURLs.ToList())
            {
                WebhookUtils.SendWebhook(webhookURL.WebhookURL, new WebhookMessage {
                    Content = msg.Message,
                    Username = msg.Author,
                });
            }
            consumed = true; // Indicating the message has been processed
        }

        public static void Load() {
            Log.Info("Loading event handlers");
            ManagerUtils.ChatManagerServer.MessageRecieved += OnMessageRecieved;
        }

        public static void Unload() {
            Log.Info("Unloading event handlers");
            ManagerUtils.ChatManagerServer.MessageRecieved -= OnMessageRecieved;
        }
    }
}
