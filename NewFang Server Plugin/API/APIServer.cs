using NewFangServerPlugin.API.Server;
using NewFangServerPlugin.API.Torch;
using NewFangServerPlugin.Configs;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.Utils.Webs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRageMath;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace NewFangServerPlugin.API {
    public class APIServer {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        private List<string> _connectedWebhookURLs => PluginInstance.Config.ConnectedWebhookURLs.Select(webhook => webhook.WebhookURL).ToList();

        private WebserverLite _server;

        public ushort Port { get; set; }


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
            Port = (ushort)port;
            StartServer();
        }

        public void RestartServerWithPort(ushort port) {
            StopServer();
            Port = port;
            StartServer();
        }

        public static async Task APIExceptionHandler(HttpContextBase ctx, Exception exception) {
            Log.Error(exception);
            ctx.Response.StatusCode = 500;
            await ctx.Response.Send($"Internal Server Error: {exception.Message}");
        }

        async Task DefaultRoute(HttpContextBase ctx) =>
            await ctx.Response.Send("NewFang Server Plugin API Server v1.0");

        void StartServer() {
            // Check the url to check the port not in use
            if(PortChecker.IsPortInUse(Port)) {
                Log.Error($"Port {Port} is already in use! Please change the port in the config file.");
                foreach(ConnectedWebhookURL url in PluginInstance.Config.ConnectedWebhookURLs) {
                    WebhookUtils.SendWebhook(url.WebhookURL, new WebhookMessage() {
                        Content = $"Port {Port} is already in use! Please change the port in the config file.",
                        Username = "Server"
                    });
                }
                return;
            } else {
                Log.Info($"Port {Port} is not in use.");
            }

            _server = new WebserverLite(new WebserverSettings("localhost", Port), DefaultRoute);

            ServerStatusAPIRoutes.SetupServerStatusRoutes(_server);
            MessageAPIRoutes.SetupMessageAPIRoutes(_server);
            ModsAPIRoutes.SetupModsAPIRoutes(_server);
            WebhooksAPIRoutes.SetupWebhooksAPIRoutes(_server);
            TorchPluginAPIRoutes.SetupTorchPluginAPIRoutes(_server);
            PlayersAPIRoutes.SetupPlayersAPIRoutes(_server);

            _server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/vaildate/apikey", async ctx => {
                string apiKey = ctx.Request.Query.Elements["APIKey"];
                if(PluginInstance.Config.APIKey == apiKey) {
                    await ctx.Response.Send("API Key is valid!");
                } else {
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.Send("Unauthorized: Invalid API Key");
                }
            });

            _server.Start();

            Log.Info($"API Server started at localhost:{Port}");
        }

        void StopServer() {
            if(_server != null) {
                // Stop the server and dispose of it
                _server.Dispose();
                _server = null;
                Log.Info("API Server stopped.");
            } else {
                Log.Warn("API Server is not running.");
            }
        }

        public void CallSendGetRequest(ulong playerID, string route, string parameters = null) {
            Task.Run(async () => await SendGetRequest(playerID, route, parameters)).Wait();
        }

        public void CallSendDeleteRequest(ulong playerID, string route, string parameters = null) {
            Task.Run(async () => await SendDeleteRequest(playerID, route, parameters)).Wait();
        }

        private async Task SendDeleteRequest(ulong playerID, string route, string parameters = null) {
            try {
                string parms = parameters != null
                    ? string.Join("&", parameters.Split(','))
                    : string.Empty;

                string newURL = $"http://localhost:{Port}{route}?APIKey={PluginInstance.Config.APIKey}&{parms}";
                Log.Info($"Sending DELETE Request to {newURL}");

                using(var client = new System.Net.Http.HttpClient()) {
                    var response = await client.DeleteAsync(newURL);
                    response.EnsureSuccessStatusCode();

                    string responseString = $"\n{await response.Content.ReadAsStringAsync()}";
                    Log.Info($"Response: {responseString}");

                    ManagerUtils.ChatManagerServer?.SendMessageAsOther("Server", responseString, default(Color), playerID);
                }
            } catch(Exception e) {
                ManagerUtils.ChatManagerServer?.SendMessageAsOther("Server", $"Error: {e.Message}", default(Color), playerID);
                Log.Error(e);
            }
        }

        private async Task SendGetRequest(ulong playerID, string route, string parameters = null) {
            try {
                string parms = parameters != null
                    ? string.Join("&", parameters.Split(','))
                    : string.Empty;

                string newURL = $"http://localhost:{Port}{route}?APIKey={PluginInstance.Config.APIKey}&{parms}";
                Log.Info($"Sending GET Request to {newURL.Replace(PluginInstance.Config.APIKey, "[REDACTED]")}");

                using(var client = new System.Net.Http.HttpClient()) {
                    var response = await client.GetAsync(newURL);
                    response.EnsureSuccessStatusCode();

                    string responseString = $"\n{await response.Content.ReadAsStringAsync()}";
                    Log.Info($"Response: {responseString}");

                    ManagerUtils.ChatManagerServer?.SendMessageAsOther("Server", responseString, default(Color), playerID);
                }
            } catch(Exception e) {
                ManagerUtils.ChatManagerServer?.SendMessageAsOther("Server", $"Error: {e.Message}", default(Color), playerID);
                Log.Error(e);
            }
        }
    }
}
