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
            server.Routes.PreAuthentication.Parameter.Add(HttpMethod.GET, "/api/v1/webhooks/{webhook.id}/{webhook.token}", AttachWebhookRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Parameter.Add(HttpMethod.DELETE, "/api/v1/webhooks/{webhook.id}/{webhook.token}", DeAttachWebhookRoute, APIServer.APIExceptionHandler);
        }
        static async Task AttachWebhookRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            if(string.IsNullOrEmpty(ctx.Request.Url.Parameters["webhook.id"]) || string.IsNullOrEmpty(ctx.Request.Url.Parameters["webhook.token"])) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook ID and Token are required.");
                return;
            }

            string url = $"https://discord.com/api/webhooks/{ctx.Request.Url.Parameters["webhook.id"]}/{ctx.Request.Url.Parameters["webhook.token"]}";

            if(string.IsNullOrEmpty(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: URL is required.");
                return;
            } else if(_connectedWebhookURLs.Contains(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook already attached.");
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

            if(string.IsNullOrEmpty(ctx.Request.Url.Parameters["webhook.id"]) || string.IsNullOrEmpty(ctx.Request.Url.Parameters["webhook.token"])) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook ID and Token are required.");
                return;
            }

            string url = $"https://discord.com/api/webhooks/{ctx.Request.Url.Parameters["webhook.id"]}/{ctx.Request.Url.Parameters["webhook.token"]}";

            if(string.IsNullOrEmpty(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: URL is required.");
                return;
            } else if(_connectedWebhookURLs.Contains(url)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Webhook already attached.");
                return;
            }

            PluginInstance.Config.ConnectedWebhookURLs.Remove(PluginInstance.Config.ConnectedWebhookURLs.First(webhook => webhook.WebhookURL == url));
            PluginInstance.Save();
        }

    }
}
