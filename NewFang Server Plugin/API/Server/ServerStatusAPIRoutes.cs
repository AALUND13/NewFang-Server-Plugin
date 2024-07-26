using NewFangServerPlugin.Handler;
using NewFangServerPlugin.Structs;
using NewFangServerPlugin.Utils;
using NLog;
using System;
using System.Threading.Tasks;
using Torch.API.Session;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace NewFangServerPlugin.API.Server {
    public static class ServerStatusAPIRoutes {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        public static void SetupServerStatusRoutes(WebserverLite server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/status", GetStatusRoute, APIServer.APIExceptionHandler);

            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/start", StartRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/stop", StopRoute, APIServer.APIExceptionHandler);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/restart", RestartRoute, APIServer.APIExceptionHandler);
        }

        static async Task GetStatusRoute(HttpContextBase ctx) {
            ServerStatus status = ServerStatusUtil.GetServerStatus();
            string json = status.ToJson();

            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(json);
        }

        static async Task StartRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            if(ManagerUtils.Torch.CanRun) {
                await ctx.Response.Send("Server starting!");
                ManagerUtils.Torch.Start();
            } else {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Server is already running.");
            }
        }

        static async Task StopRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            if(!ManagerUtils.Torch.IsRunning) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Server is not running.");
            }

            GameSaveResult saveResult = await ManagerUtils.Torch.Save(exclusive: true);
            if(saveResult == GameSaveResult.Success) {
                await ctx.Response.Send("Server stopping!");
                ManagerUtils.Torch.Stop();
            } else {
                ctx.Response.StatusCode = 500;
                await ctx.Response.Send("Internal Server Error: Failed to save the game.");
            }
        }

        static async Task RestartRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            bool success = int.TryParse(ctx.Request.Query.Elements["Delay"], out int delay);
            if(!success) delay = 0;

            (RestartTimer restartTimer, Exception exception) = SafeMethodExecutor.ExecuteSafe(() => new RestartTimer(delay));

            if(exception != null && exception is TimerAlreadyActiveException timerAlreadyActiveException) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Server is already restarting.");
            } else if(exception == null) {
                await ctx.Response.Send("Server restarting!");
            } else {
                throw exception;
            }
        }
    }
}
