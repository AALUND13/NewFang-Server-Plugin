using NewFangServerPlugin.Structs;
using NewFangServerPlugin.Utils;
using Newtonsoft.Json;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System.Linq;
using System.Threading.Tasks;
using Torch.Server.ViewModels;
using Torch.Utils.SteamWorkshopTools;
using Torch.ViewModels;
using VRage.Game;
using VRage.Game.ModAPI;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace NewFangServerPlugin.API.Server {
    public static class PlayersAPIRoutes {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        public static void SetupPlayersAPIRoutes(Webserver server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/players/promote", PromotePlayerRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/players/demote", DemotePlayerRoute, APIServer.APIExceptionHandler);
        }

        static async Task PromotePlayerRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            if(!ManagerUtils.Torch.IsRunning) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Server is not running.");
            }

            string PlayerName = ctx.Request.Query.Elements["PlayerName"];
            if(string.IsNullOrEmpty(PlayerName)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: PlayerName is required.");
                return;
            }
            PlayerViewModel player = ManagerUtils.MultiplayerManager.Players.Values.FirstOrDefault(player => player.Name.StartsWith(PlayerName));
            if(player == null) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Player not found.");
                return;
            }

            #pragma warning disable CS0472
            if(player.SteamId == null) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Player has no SteamId.");
                return;
            }
            #pragma warning restore CS0472

            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(JsonConvert.SerializeObject(PlayerRankChange.PromotePlayer(player.SteamId)));
        }

        static async Task DemotePlayerRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            if(!ManagerUtils.Torch.IsRunning) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Server is not running.");
            }

            string PlayerName = ctx.Request.Query.Elements["PlayerName"];
            if(string.IsNullOrEmpty(PlayerName)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: PlayerName is required.");
                return;
            }

            PlayerViewModel player = ManagerUtils.MultiplayerManager.Players.Values.FirstOrDefault(player => player.Name.StartsWith(PlayerName));
            if(player == null) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Player not found.");
                return;
            }

            #pragma warning disable CS0472
            if(player.SteamId == null) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Player has no SteamId.");
                return;
            }
            #pragma warning restore CS0472

            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(JsonConvert.SerializeObject(PlayerRankChange.DemotePlayer(player.SteamId)));
        }
    }
}
