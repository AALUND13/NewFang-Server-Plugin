using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NewFangServerPlugin.Utils;
using NewFangServerPlugin.WebAPI.Models;
using NLog;

namespace NewFangServerPlugin.WebAPI {
    public class ChatApiController : WebApiController {
        private NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Route(HttpVerbs.Post, "/chat/send")]
        public MessageRespone SendMessage([JsonData] ChatMessageRequest request) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(string.IsNullOrEmpty(request.Message))
                throw HttpException.BadRequest("Message are required.");
            else if(!string.IsNullOrEmpty(request.Sender))
                ManagerUtils.ChatManagerServer?.SendMessageAsOther(request.Sender, request.Message, VRageMath.Color.White);
            else
                ManagerUtils.ChatManagerServer?.SendMessageAsSelf(request.Message);

            Log.Info($"Message sent by [{(!string.IsNullOrEmpty(request.Sender) ? request.Sender : "Server")}]: {request.Message}");
            return new MessageRespone("Message sent!");
        }

        [Route(HttpVerbs.Post, "/chat/command")]
        public MessageRespone SendCommand([JsonData] ChatCommandRequest request) {
            if(PluginInstance.Config.APIKey != Request.QueryString["key"])
                throw HttpException.Unauthorized("Invalid API Key");
            else if(string.IsNullOrEmpty(request.Command))
                throw HttpException.BadRequest("Command are required.");
            else if(request.Command != null && !ManagerUtils.CommandManager.IsCommand($"!{request.Command}"))
                throw HttpException.BadRequest("Invalid command.");
            else if(!ManagerUtils.CommandManager.HandleCommandFromServer($"!{request.Command}", null))
                throw HttpException.InternalServerError("Failed to execute the command.");

            Log.Info($"Command executed by [Server]: {request.Command}");
            return new MessageRespone("Command executed!");
        }
    }
}
