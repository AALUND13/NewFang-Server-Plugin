using NewFangServerPlugin.API.Server;
using NewFangServerPlugin.API.Torch;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace NewFangServerPlugin.API {
    public class APIServer {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        private List<string> _connectedWebhookURLs => PluginInstance.Config.ConnectedWebhookURLs.Select(webhook => webhook.WebhookURL).ToList();

        /**
        This class provides various server-related operations:
         
        - Server
         - Status: Check the current status of the server.
         
         - Start: Start the server.
         - Stop: Stop the server.
         - Restart: Restart the server.
         
         - Mods
          - Add: Add a mod to the server.
          - Remove: Remove a mod from the server.

         - Message
          - Send: Send a message to the server or its clients.
        
        - Torch
         - Plugins
          - Add: Add a plugin to the server.
          - List: List all plugins on the server.
          - Remove: Remove a plugin from the server.

        - Webhooks
         - Attach: Attach webhooks for various events.
         - Deattach: Deattach webhooks.
        **/

        public APIServer(string hostName = "127.0.0.1", int port = 9860) {
            Webserver server = new Webserver(new WebserverSettings(hostName, port), DefaultRoute);

            ServerStatusAPIRoutes.SetupServerStatusRoutes(server);
            MessageAPIRoutes.SetupMessageAPIRoutes(server);
            ModsAPIRoutes.SetupModsAPIRoutes(server);
            WebhooksAPIRoutes.SetupWebhooksAPIRoutes(server);
            TorchPluginAPIRoutes.SetupTorchPluginAPIRoutes(server);
            PlayersAPIRoutes.SetupPlayersAPIRoutes(server);

            server.Start();

            Log.Info($"API Server started at {hostName}:{port}");
        }

        public static async Task APIExceptionHandler(HttpContextBase ctx, Exception exception) {
            Log.Error(exception);
            ctx.Response.StatusCode = 500;
            await ctx.Response.Send($"Internal Server Error: {exception.Message}");
        }

        async Task DefaultRoute(HttpContextBase ctx) =>
            await ctx.Response.Send("NewFang Server Plugin API Server v1.0");
    }
}
