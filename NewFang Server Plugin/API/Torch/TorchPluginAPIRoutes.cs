using NewFangServerPlugin.Utils;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Torch;
using Torch.API.WebAPI;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace NewFangServerPlugin.API.Torch {
    internal class TorchPluginAPIRoutes {

        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;


        public static void SetupTorchPluginAPIRoutes(Webserver server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/torch/plugin/add", PluginAddRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/torch/plugin/list", ListPluginsRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/torch/plugin/remove", PluginRemoveRoute, APIServer.APIExceptionHandler);
        }
        static async Task PluginAddRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string id = ctx.Request.Query.Elements["ID"];
            if(string.IsNullOrEmpty(id)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: ID is required.");
                return;
            }

            bool success = Guid.TryParse(id, out Guid guid);

            if(!success) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Invalid ID.");
                return;
            }

            if(ManagerUtils.PluginsManager.Plugins.ContainsKey(guid)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Plugin already exists.");
                return;
            }

            PluginFullItem respone = await PluginQuery.Instance.QueryOne(id);

            if(respone == null) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Plugin not found.");
                return;
            }

            if(!await PluginQuery.Instance.DownloadPlugin(respone)) {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send("Internal Server Error: Failed to download plugin.");
                return;
            }

            ManagerUtils.Torch.Config.Plugins.Add(guid);
            await ctx.Response.Send("Plugin added.");
        }

        static async Task ListPluginsRoute(HttpContextBase ctx) {
            string pluginNameKeyPairsJson = JsonConvert.SerializeObject(ManagerUtils.PluginsManager.Plugins.Select(plugin => new
            {
                Name = plugin.Value.Name,
                Guid = plugin.Key.ToString()
            }));

            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(pluginNameKeyPairsJson);
        }

        static async Task PluginRemoveRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string id = ctx.Request.Query.Elements["ID"];
            if(string.IsNullOrEmpty(id)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: ID is required.");
                return;
            }

            bool success = Guid.TryParse(id, out Guid guid);

            if(!success) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Invalid ID.");
                return;
            }

            if(!ManagerUtils.PluginsManager.Plugins.ContainsKey(guid)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Plugin not found.");
                return;
            }

            foreach(string zip in Directory.EnumerateFiles(ManagerUtils.PluginsManager.PluginDir, "*.zip")) {
                PluginManifest manifest = PluginManifestUtils.ReadFromZip(zip);
                if(manifest.Guid == guid) {
                    File.Delete(zip);
                    await ctx.Response.Send("Plugin removed.");
                    return;
                }
            }

            ctx.Response.StatusCode = 500;
            await ctx.Response.Send("Internal Server Error: Failed to remove plugin.");
        }

    }
}
