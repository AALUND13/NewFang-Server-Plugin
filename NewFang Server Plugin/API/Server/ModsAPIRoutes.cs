using NewFangServerPlugin.Utils;
using NLog;
using System.Linq;
using System.Threading.Tasks;
using Torch.Server.ViewModels;
using Torch.Utils.SteamWorkshopTools;
using VRage.Game;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace NewFangServerPlugin.API.Server {
    public static class ModsAPIRoutes {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void SetupModsAPIRoutes(WebserverLite server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/mods/add", AddModRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/mods/remove", RemoveModRoute, APIServer.APIExceptionHandler);
        }

        static async Task RemoveModRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string ModID = ctx.Request.Query.Elements["ModID"];
            if(string.IsNullOrEmpty(ModID)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: ModID is required.");
                return;
            }

            ulong modID = ulong.Parse(ModID);
            if(!ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId.ToString() == ModID)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Mod does not exist.");
                return;
            }

            ManagerUtils.InstanceManager.DedicatedConfig.Mods.RemoveWhere(mod => mod.PublishedFileId.ToString() == ModID);
            Log.Info($"Mod removed: {ModID}, Restart the server to apply changes.");
            await ctx.Response.Send("Mod removed! Restart the server to apply changes.");
        }

        static async Task AddModRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string ModID = ctx.Request.Query.Elements["ModID"];
            if(string.IsNullOrEmpty(ModID)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: ModID is required.");
                return;
            }

            ulong modID = ulong.Parse(ModID);
            if(ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId.ToString() == ModID)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Mod already exists.");
                return;
            }

            PublishedItemDetails modInfo;
            try {
                modInfo = await ModUtil.GetModInfoByID(modID);
            } catch(ModUtil.NotSEWorkshopItemException) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: The item is not a Space Engineers workshop item.");
                return;
            } catch(ModUtil.NotModException) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: The item is not a mod.");
                return;
            }

            ManagerUtils.InstanceManager.DedicatedConfig.Mods.Add(new ModItemInfo(new MyObjectBuilder_Checkpoint.ModItem() {
                PublishedFileId = modID,
                FriendlyName = modInfo.Title,
            }));

            Log.Info($"Mod added: {modID}, Restart the server to apply changes.");
            await ctx.Response.Send("Mod added! Restart the server to apply changes.");
        }

    }
}
