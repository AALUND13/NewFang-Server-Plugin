using NewFangServerPlugin.Utils;
using NLog;
using System.Threading.Tasks;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace NewFangServerPlugin.API.Server {
    public static class MessageAPIRoutes {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void SetupMessageAPIRoutes(WebserverLite server) {
            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/api/v1/server/message/send", SendMessageRoute, APIServer.APIExceptionHandler);
        }

        static async Task SendMessageRoute(HttpContextBase ctx) {
            if(PluginInstance.Config.APIKey != ctx.Request.Query.Elements["APIKey"]) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.Send("Unauthorized: Invalid API Key");
                return;
            }

            string Sender = ctx.Request.Query.Elements["Sender"];
            string Message = ctx.Request.Query.Elements["Message"];

            if(string.IsNullOrEmpty(Message)) {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send("Bad Request: Message are required.");
                return;
            }

            if(string.IsNullOrEmpty(Sender)) {
                ManagerUtils.ChatManagerServer?.SendMessageAsSelf(Message);
                Log.Info($"Message sent by [Server]: {Message}");
            } else {
                ManagerUtils.ChatManagerServer?.SendMessageAsOther(Sender, Message, VRageMath.Color.White);
                Log.Info($"Message sent by [{Sender}]: {Message}");
            }

            await ctx.Response.Send("Message sent!");
        }

    }
}
