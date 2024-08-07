using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NewFangServerPlugin.Handler;
using NewFangServerPlugin.Structs;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.WebAPI.Models;
using NLog;
using System;
using System.Threading.Tasks;
using System.Timers;
using Torch.API.Session;


#pragma warning disable CS4014
namespace NewFangServerPlugin.WebAPI {
    public class ServerApiController : WebApiController {
        private NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route(HttpVerbs.Get, "/server/status")]
        public ServerStatus GetServerStatus() {
            return ServerStatusUtil.GetServerStatus();
        }

        [Route(HttpVerbs.Post, "/server/start")]
        public MessageRespone StartServer() {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(!ManagerUtils.Torch.CanRun)
                throw HttpException.BadRequest("Server is already running.");

            // Start the server after 1 second, So the response can be sent.
            Task.Run(async () => {
                await Task.Delay(1000);
                ManagerUtils.Torch.Start();
            });

            return new MessageRespone("Server starting!");
        }

        [Route(HttpVerbs.Post, "/server/stop")]
        public async Task<MessageRespone> StopServer() {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(!ManagerUtils.Torch.IsRunning)
                throw HttpException.BadRequest("Server is not running.");

            GameSaveResult saveResult = await ManagerUtils.Torch.Save(exclusive: true);
            if(saveResult != GameSaveResult.Success)
                throw HttpException.InternalServerError("Failed to save the game.");

            // Stop the server after 1 second, So the response can be sent.
            Task.Run(async () => {
                await Task.Delay(1000);
                ManagerUtils.Torch.Stop();
            });

            return new MessageRespone("Server stopping!");
        }

        [Route(HttpVerbs.Post, "/server/restart")]
        public MessageRespone RestartServer([QueryField] int delay = 0) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(RestartTimer.IsAlreadyActive())
                throw HttpException.BadRequest("Server is already restarting.");

            // Restart the server after the delay with 1 second extra, So the response can be sent.
            new RestartTimer(Math.Max(delay, 0) + 1);
            return new MessageRespone("Server restarting!");
        }
    }
}
#pragma warning restore CS4014
