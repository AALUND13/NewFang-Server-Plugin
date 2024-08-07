using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NewFangServerPlugin.Configs;
using NewFangServerPlugin.WebAPI.Models;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace NewFangServerPlugin.WebAPI {
    public class WebhooksAPIController : WebApiController {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static List<string> _connectedWebhookURLs => PluginInstance.Config.ConnectedWebhookURLs.Select(webhook => webhook.WebhookURL).ToList();

        [Route(HttpVerbs.Put, "/webhooks/{webhookId}/{webhookToken}")]
        public MessageRespone AddOrConfigureWebhook(string webhookId, string webhookToken) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            string url = $"https://discord.com/api/webhooks/{webhookId}/{webhookToken}";
            if(_connectedWebhookURLs.Contains(url))
                throw HttpException.BadRequest("Webhook already attached.");

            PluginInstance.Config.ConnectedWebhookURLs.Add(new ConnectedWebhookURL() { WebhookURL = url });
            PluginInstance.Save();

            return new MessageRespone("Webhook attached!");
        }

        [Route(HttpVerbs.Delete, "/webhooks/{webhookId}/{webhookToken}")]
        public MessageRespone RemoveWebhook(string webhookId, string webhookToken) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            string url = $"https://discord.com/api/webhooks/{webhookId}/{webhookToken}";
            if(!_connectedWebhookURLs.Contains(url))
                throw HttpException.NotFound("Webhook not found.");

            PluginInstance.Config.ConnectedWebhookURLs.Remove(PluginInstance.Config.ConnectedWebhookURLs.First(webhook => webhook.WebhookURL == url));
            PluginInstance.Save();

            return new MessageRespone("Webhook deattached!");
        }
    }
}
