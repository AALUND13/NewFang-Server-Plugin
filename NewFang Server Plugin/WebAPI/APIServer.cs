using EmbedIO;
using EmbedIO.WebApi;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.Utils.Webs;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VRageMath;

namespace NewFangServerPlugin.WebAPI {
    public class APIServer {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;

        public readonly Logger Log = LogManager.GetCurrentClassLogger();
        public ushort Port { get; set; }

        private WebServer _server;

        //private List<string> _connectedWebhookURLs => PluginInstance.Config.ConnectedWebhookURLs.Select(webhook => webhook.WebhookURL).ToList();

        /**
        This class provides various server-related operations:
         
        - Server
         - Status: Check the current status of the server.
         
         - Start: Start the server. (Post)
         - Stop: Stop the server. (Post)
         - Restart: Restart the server. (Post)
        
        - Chat
         - Send: Send a chat message to the server. (Post) {Message, Sender?}
         - Command: Send a command to the server. (Post) {Command}

        - Mods
         - {id}: Add a mod to the server. (Put)
         - {id}: Remove a mod from the server. (Delete)

        - Players
         - {id}/promote: Promote a player. (Post)
         - {id}/demote: Demote a player. (Post)

        - Plugins: Get a list of plugins. (Get)
         - {id}: Add a plugin to the server. (Put)
         - {id}: Remove a plugin from the server. (Delete)

        - Webhooks
         - {webhookId}/{webhookToken}: Add a webhook to the server. (Put)
         - {webhookId}/{webhookToken}: Remove a webhook from the server. (Delete)
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

        void StartServer() {
            bool portInUse = PortChecker.IsPortInUse(Port);
            if(portInUse) {
                Log.Warn($"Port {Port} is already in use.");
                return;
            }

            _server = CreateWebServer();
            Task.Run(() => _server.RunAsync());
            Log.Info($"API Server started at http://localhost:{Port}");
        }

        WebServer CreateWebServer() {
            var webApiModule = new WebApiModule("/api/v1", ResponseSerializer.Json)
                .HandleUnhandledException(async (ctx, ex) => {
                    Log.Error(ex);
                    throw HttpException.InternalServerError(ex.Message, ex.Data);
                });

            // Create a WebServer instance and configure routing
            var server = new WebServer(o => o
                .WithUrlPrefix($"http://localhost:{Port}")
                .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithModule(webApiModule, m => m
                .WithController<ServerApiController>()
                .WithController<ChatApiController>()
                .WithController<ModsAPIController>()
                .WithController<PluginsAPIController>()
                .WithController<WebhooksAPIController>()
            )
            .WithCors(); // Optional: Add CORS if needed

            return server;
        }

        void StopServer() {
            if(_server != null) {
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

        public void CallSendPostRequest(ulong playerID, string route, string parameters = null) {
            Task.Run(async () => await SendPostRequest(playerID, route, parameters)).Wait();
        }

        public void CallSendPutRequest(ulong playerID, string route, string parameters = null) {
            Task.Run(async () => await SendPutRequest(playerID, route, parameters)).Wait();
        }

        public void CallSendDeleteRequest(ulong playerID, string route, string parameters = null) {
            Task.Run(async () => await SendDeleteRequest(playerID, route, parameters)).Wait();
        }

        private async Task SendPostRequest(ulong playerID, string route, string parameters = null) {
            try {
                string parms = parameters != null
                    ? string.Join("&", parameters.Split(','))
                    : string.Empty;

                string newURL = $"http://localhost:{Port}{route}?key={PluginInstance.Config.APIKey}&{parms}";
                Log.Info($"Sending DELETE Request to {newURL}");

                using(var client = new System.Net.Http.HttpClient()) {
                    var response = await client.PostAsync(newURL, null);
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

        private async Task SendPutRequest(ulong playerID, string route, string parameters = null) {
            try {
                string parms = parameters != null
                    ? string.Join("&", parameters.Split(','))
                    : string.Empty;

                string newURL = $"http://localhost:{Port}{route}?key={PluginInstance.Config.APIKey}&{parms}";
                Log.Info($"Sending DELETE Request to {newURL}");

                using(var client = new System.Net.Http.HttpClient()) {
                    var response = await client.PutAsync(newURL, null);
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

        private async Task SendDeleteRequest(ulong playerID, string route, string parameters = null) {
            try {
                string parms = parameters != null
                    ? string.Join("&", parameters.Split(','))
                    : string.Empty;

                string newURL = $"http://localhost:{Port}{route}?key={PluginInstance.Config.APIKey}&{parms}";
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

                string newURL = $"http://localhost:{Port}{route}?key={PluginInstance.Config.APIKey}&{parms}";
                Log.Info($"Sending GET Request to {newURL.Replace(PluginInstance.Config.APIKey, "[REDACTED]")}");

                using(var client = new System.Net.Http.HttpClient()) {
                    var response = await client.GetAsync(newURL);

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
