using NLog;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Torch.Server.Managers;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Session;
using Torch.Server.ViewModels;
using System.Diagnostics;
using System.Management;
using Torch.Server;
using Torch.API;
using NewFangServerPlugin.Utils;

namespace NewFangServerPlugin.API {
    public struct ModInfo {
        public ulong ModID;
        public string ModName;

        public ModInfo(ulong modID, string modName) {
            ModID = modID;
            ModName = modName;
        }
    }

    public struct ServerStatus {
        public bool IsRunning;

        public string ServerName;

        public string ServerPublicIP;
        public int Port;

        public int MaxPlayers;
        public List<string> Players;

        public List<ModInfo> ModList;

        public float SimulationSpeed;
        public string Uptime;

        public string ToJson() {
            JsonSerializerSettings settings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,

            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }
    public static class ServerStatusUtil {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        public static ServerStatus GetServerStatus() {
            ServerStatus status = new ServerStatus {
                IsRunning = ManagerUtils.Torch.IsRunning,

                ServerName = MySandboxGame.ConfigDedicated.ServerName,
                
                ServerPublicIP = new IPAddress(BitConverter.GetBytes(ManagerUtils.Torch.IsRunning ? MyGameService.GameServer.GetPublicIP() : 0).Reverse().ToArray()).ToString(),
                Port = MySandboxGame.ConfigDedicated.ServerPort,
                
                MaxPlayers = MySandboxGame.ConfigDedicated.SessionSettings.MaxPlayers,
                Players = MySession.Static?.Players?
                .GetOnlinePlayers()
                .Where(x => x.IsRealPlayer && !string.IsNullOrEmpty(x.DisplayName))
                .Select(p => p.DisplayName)
                .ToList() ?? new List<string>(),

                ModList = ManagerUtils.InstanceManager.DedicatedConfig.Mods
                .Select(m => new ModInfo(m.PublishedFileId, m.FriendlyName))
                .ToList() ?? null,
                
                SimulationSpeed = ManagerUtils.Torch.IsRunning ? Sync.ServerSimulationRatio : 0,
                Uptime = ((ITorchServer)PluginInstance.Torch).ElapsedPlayTime.ToString()
            };
            
            // Get the mods without the server be online

            return status;
        }
    }
}
