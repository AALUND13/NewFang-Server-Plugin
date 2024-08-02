using NewFangServerPlugin.Configs;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.Utils.Webs;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using System.Collections.Generic;
using System.Linq;
using Torch.API.Managers;
using VRage.GameServices;

namespace NewFangServerPlugin.Handler {
    public class PluginEventHandler {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static Dictionary<ulong, string> steamIdToUsernameDictionary = new Dictionary<ulong, string>();

        public static void OnMessageRecieved(TorchChatMessage msg, ref bool consumed) {
            if(msg.Channel != ChatChannel.Global) return; // Only process global chat messages

            foreach(ConnectedWebhookURL webhookURL in PluginInstance.Config.ConnectedWebhookURLs.ToList()) {
                WebhookUtils.SendWebhook(webhookURL.WebhookURL, new WebhookMessage {
                    Content = msg.Message,
                    Username = msg.Author,
                });
            }
            consumed = true; // Indicating the message has been processed
        }

        public static void ClientJoined(ulong steamID, string playerName) {
            steamIdToUsernameDictionary.Add(steamID, playerName);

            foreach(ConnectedWebhookURL webhookURL in PluginInstance.Config.ConnectedWebhookURLs.ToList()) {
                WebhookUtils.SendWebhook(webhookURL.WebhookURL, new WebhookMessage {
                    Content = $"**{playerName.Substring(1)}** has joined the server.",
                    Username = "Server",
                });
            }
        }

        public static void ClientLeft(ulong steamID, MyChatMemberStateChangeEnum memberStateChangeEnum) {
            if(!steamIdToUsernameDictionary.ContainsKey(steamID)) return;

            foreach(ConnectedWebhookURL webhookURL in PluginInstance.Config.ConnectedWebhookURLs.ToList()) {
                bool beBannedOrKicked = memberStateChangeEnum == MyChatMemberStateChangeEnum.Kicked || memberStateChangeEnum == MyChatMemberStateChangeEnum.Banned;
                WebhookUtils.SendWebhook(webhookURL.WebhookURL, new WebhookMessage {
                    Content = $"**{steamIdToUsernameDictionary[steamID].Substring(1)}** {(beBannedOrKicked ? "was" : "has")} {memberStateChangeEnum.ToString().ToLower()}.",
                    Username = "Server",
                });
            }

            steamIdToUsernameDictionary.Remove(steamID);
        }

        public static void Load() {
            Log.Info("Loading event handlers");
            ManagerUtils.ChatManagerServer.MessageRecieved += OnMessageRecieved;
            MyMultiplayer.Static.ClientJoined += ClientJoined;
            MyMultiplayer.Static.ClientLeft += ClientLeft;
        }

        public static void Unload() {
            Log.Info("Unloading event handlers");
            ManagerUtils.ChatManagerServer.MessageRecieved -= OnMessageRecieved;
            MyMultiplayer.Static.ClientJoined -= ClientJoined;
            MyMultiplayer.Static.ClientLeft -= ClientLeft;
        }
    }
}
