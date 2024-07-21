using NewFangServerPlugin.Configs;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace NewFangServerPlugin.API {
    public class WebhooksAPIRoutes {

        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;
        private static List<string> _connectedWebhookURLs => PluginInstance.Config.ConnectedWebhookURLs.Select(webhook => webhook.WebhookURL).ToList();

        public static void SetupWebhooksAPIRoutes(Webserver server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/webhooks/attach", AttachWebhookRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/webhooks/deattach", DeAttachWebhookRoute, APIServer.APIExceptionHandler);
        }
        static async Task AttachWebhookRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string url = ctx.Request.Query.Elements["URL"];

            if(string.IsNullOrEmpty(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: URL is required.");
                return;
            } else if(_connectedWebhookURLs.Contains(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook already attached.");
                return;
            } else if(!url.StartsWith("https://discord.com/api/webhooks/")) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Invalid Discord Webhook URL.");
                return;
            }

            PluginInstance.Config.ConnectedWebhookURLs.Add(new ConnectedWebhookURL() { WebhookURL = url });
            PluginInstance.Save();

            await ctx.Response.Send("Webhook attached!");
        }

        static async Task DeAttachWebhookRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string url = ctx.Request.Query.Elements["URL"];

            if(string.IsNullOrEmpty(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: URL is required.");
                return;
            } else if(!_connectedWebhookURLs.Contains(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook already not attached.");
                return;
            } else if(!url.StartsWith("https://discord.com/api/webhooks/")) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Invalid Discord Webhook URL.");
                return;
            }

            PluginInstance.Config.ConnectedWebhookURLs.Remove(PluginInstance.Config.ConnectedWebhookURLs.First(webhook => webhook.WebhookURL == url));
            PluginInstance.Save();
        }

    }
}
