using NewFangServerPlugin;
using Sandbox.Game.Multiplayer;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace NewFangServerPlugin {
    [Category("RemoteAPI")]
    public class NewFang_Server_PluginCommands : CommandModule {

        public NewFangServerPlugin Plugin => NewFangServerPlugin.Instance;
        
        private ulong DeveloperID = 76561198360315155;

        [Command("Restart", "Restart the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void RestartAPI(ushort port = 7167) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                Plugin.Config.APIPort = port;
                Plugin.Save();

                Plugin.APIServer.RestartServerWithPort(port);
                Context.Respond("API Server Restarted!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }

        [Command("SendGetRequest", "Send a GET Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendGetRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                Plugin.APIServer.CallSendGetRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }

        [Command("SendDeleteRequest", "Send a DELETE Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendDeleteRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                Plugin.APIServer.CallSendDeleteRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }
    }
}
