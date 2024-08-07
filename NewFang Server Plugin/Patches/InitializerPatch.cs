using HarmonyLib;
using NewFangServerPlugin.Utils.Webs;
using System;
using System.Linq;
using Torch.Server;

namespace NewFangServerPlugin.Patches {
    [HarmonyPatch(typeof(Initializer))]
    public class InitializerPatch {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;

        [HarmonyPatch("HandleException")]
        [HarmonyPrefix]
        public static async void Prefix(object sender, UnhandledExceptionEventArgs e) {
            Exception exception = (Exception)e.ExceptionObject;

            foreach(var webhookURL in PluginInstance.Config.ConnectedWebhookURLs.ToList()) {
                await WebhookUtils.SendWebhookAsync(webhookURL.WebhookURL, new WebhookMessage {
                    Content = $"Server has crashed! `{exception.GetType().Name}: {exception.Message}`",
                    Username = "Server",
                });
            }
        }
    }
}
