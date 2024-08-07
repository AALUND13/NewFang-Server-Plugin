using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.WebAPI.Models;
using NLog;
using Sandbox.Game.World;
using System.Linq;
using System.Threading.Tasks;
using Torch.Server.ViewModels;
using Torch.Utils.SteamWorkshopTools;
using VRage.Game;

namespace NewFangServerPlugin.WebAPI {
    public class ModsAPIController : WebApiController {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route(HttpVerbs.Put, "/mods/{id}")]
        public async Task<MessageRespone> InstallMod(ulong id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId == id))
                throw HttpException.BadRequest("Mod already installed");

            PublishedItemDetails modDetail;
            try {
                modDetail = await ModUtil.GetModInfoByID(id);
            } catch(ModUtil.NotSEWorkshopItemException) {
                throw HttpException.BadRequest("Item is not a Space Engineers workshop item");
            } catch(ModUtil.NotModException) {
                throw HttpException.BadRequest("Item don't have the \"mod\" tag");
            } catch(ModUtil.NotFoundException) {
                throw HttpException.NotFound("Mod not found");
            }

            MySession.Static?.Mods?.Add(new MyObjectBuilder_Checkpoint.ModItem() {
                Name = $"{id}.sbm",
                PublishedFileId = id,
                FriendlyName = modDetail.Title
            });
            ManagerUtils.InstanceManager.DedicatedConfig.Mods.Add(new ModItemInfo(new MyObjectBuilder_Checkpoint.ModItem() {
                Name = $"{id}.sbm",
                PublishedFileId = id,
                FriendlyName = modDetail.Title
            }));

            Log.Info($"Mod installed: {id}, Restart the server to apply changes.");
            return new MessageRespone("Mod installed! Restart the server to apply changes.");
        }

        [Route(HttpVerbs.Delete, "/mods/{id}")]
        public MessageRespone RemoveMod(ulong id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(!ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId == id))
                throw HttpException.NotFound("Mod not found");

            MySession.Static?.Mods?.RemoveAll(mod => mod.PublishedFileId == id);
            ManagerUtils.InstanceManager.DedicatedConfig.Mods.RemoveWhere(mod => mod.PublishedFileId == id);

            Log.Info($"Mod removed: {id}, Restart the server to apply changes.");
            return new MessageRespone("Mod removed! Restart the server to apply changes.");
        }
    }
}
