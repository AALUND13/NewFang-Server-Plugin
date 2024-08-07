using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.WebAPI.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Torch;
using Torch.API.WebAPI;

namespace NewFangServerPlugin.WebAPI {
    internal class PluginsAPIController : WebApiController {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route(HttpVerbs.Put, "/plugins/{id}")]
        public async Task<MessageRespone> InstallPlugin(string id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            bool success = Guid.TryParse(id, out Guid guid);
            if(!success)
                throw HttpException.BadRequest("Invalid ID");
            else if(ManagerUtils.PluginsManager.Plugins.ContainsKey(guid))
                throw HttpException.BadRequest("Plugin already exists");

            PluginFullItem respone = await PluginQuery.Instance.QueryOne(id);
            if(respone == null)
                throw HttpException.NotFound("Plugin not found");
            else if(!await PluginQuery.Instance.DownloadPlugin(respone))
                throw HttpException.InternalServerError("Failed to download plugin");

            ManagerUtils.Torch.Config.Plugins.Add(guid);
            Log.Info($"Plugin installed: {id}, Restart the server to apply changes.");
            return new MessageRespone("Plugin installed! Restart the server to apply changes.");
        }

        [Route(HttpVerbs.Delete, "/plugins/{id}")]
        public MessageRespone RemovePlugin(string id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            bool success = Guid.TryParse(id, out Guid guid);
            if(!success)
                throw HttpException.BadRequest("Invalid ID");
            else if(!ManagerUtils.Torch.Config.Plugins.Contains(guid))
                throw HttpException.NotFound("Plugin not found");

            foreach(string zip in Directory.EnumerateFiles(ManagerUtils.PluginsManager.PluginDir, "*.zip")) {
                PluginManifest manifest = PluginManifestUtils.ReadFromZip(zip);
                if(manifest.Guid == guid) {
                    File.Delete(zip);
                    Log.Info($"Plugin uninstall: {id}, Restart the server to apply changes.");
                    return new MessageRespone("Plugin uninstall! Restart the server to apply changes.");
                }
            }

            throw HttpException.InternalServerError("Failed to uninstall plugin");
        }

        [Route(HttpVerbs.Get, "/plugins")]
        public IEnumerable<List<PluginInfoResponse>> ListPlugins() {
            return ManagerUtils.PluginsManager.Plugins.Select(plugin => new List<PluginInfoResponse> {
                new PluginInfoResponse {
                    Name = plugin.Value.Name,
                    Version = plugin.Value.Version,
                    Guid = plugin.Key.ToString()
                }
            });
        }
    }
}
