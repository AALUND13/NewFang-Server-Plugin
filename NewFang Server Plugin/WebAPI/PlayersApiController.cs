using EmbedIO.WebApi;
using EmbedIO.Routing;
using EmbedIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewFangServerPlugin.WebAPI.Models;
using NLog;
using NewFangServerPlugin.Utils;
using Torch.ViewModels;
using NewFangServerPlugin.Structs;

namespace NewFangServerPlugin.WebAPI {
    public class PlayersApiController : WebApiController {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route(HttpVerbs.Post, "/players/{id}/promote")]
        public PlayerRankChange PromotePlayer(ulong id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            PlayerViewModel player = ManagerUtils.MultiplayerManager.Players.Values.ToList().Find(player => player.SteamId == id)
                ?? throw HttpException.NotFound("Player not found");

            return PlayerRankChange.PromotePlayer(player.SteamId);
        }

        [Route(HttpVerbs.Post, "/players/{id}/demote")]
        public PlayerRankChange DemotePlayer(ulong id) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");

            PlayerViewModel player = ManagerUtils.MultiplayerManager.Players.Values.ToList().Find(player => player.SteamId == id)
                ?? throw HttpException.NotFound("Player not found");

            return PlayerRankChange.DemotePlayer(player.SteamId);
        }
    }
}
