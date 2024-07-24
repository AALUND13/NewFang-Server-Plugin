﻿using NewFangServerPlugin.API.Server;
using NewFangServerPlugin.API.Torch;
using NewFangServerPlugin.Configs;
using NewFangServerPlugin.Utils.Webs;
using NLog;
using Sandbox.Engine.Utils;
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

        public APIServer(int port = 9860) {
            // Check the url to check the port not in use
            if (PortChecker.IsPortInUse(port)) {
                Log.Error($"Port {port} is already in use! Please change the port in the config file.");
                foreach(ConnectedWebhookURL url in PluginInstance.Config.ConnectedWebhookURLs) {
                    WebhookUtils.SendWebhook(url.WebhookURL, new WebhookMessage() {
                        Content = $"Port {port} is already in use! Please change the port in the config file.",
                        Username = "Server"
                    });
                }
                return;
            } else {
                Log.Info($"Port {port} is not in use.");
            }

            Webserver server = new Webserver(new WebserverSettings("localhost", port), DefaultRoute);

            ServerStatusAPIRoutes.SetupServerStatusRoutes(server);
            MessageAPIRoutes.SetupMessageAPIRoutes(server);
            ModsAPIRoutes.SetupModsAPIRoutes(server);
            WebhooksAPIRoutes.SetupWebhooksAPIRoutes(server);
            TorchPluginAPIRoutes.SetupTorchPluginAPIRoutes(server);
            PlayersAPIRoutes.SetupPlayersAPIRoutes(server);

            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/vaildate/apikey", async ctx => {
                string apiKey = ctx.Request.Query.Elements["APIKey"];
                if(PluginInstance.Config.APIKey == apiKey) {
                    await ctx.Response.Send("API Key is valid!");
                } else {
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.Send("Unauthorized: Invalid API Key");
                }
            });

            server.Start();

            Log.Info($"API Server started at localhost:{port}");
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
