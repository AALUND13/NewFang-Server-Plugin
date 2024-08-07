using NewFangServerPlugin.Utils;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Planet;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Torch;
using Torch.API.Plugins;
using Torch.API.WebAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Server.ViewModels;
using Torch.Utils.SteamWorkshopTools;
using VRage.Game;
using VRage.Game.ModAPI;

namespace NewFangServerPlugin {
    [Category("mod")]
    public class ModsCommandsCategory : CommandModule {
        private NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Command("install", "Install a mod to the server.")]
        [Permission(MyPromoteLevel.Admin)]
        public async void InstallMod(ulong modId) {
            if(ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId == modId)) {
                Context.Respond("Mod already installed");
                return;
            }

            PublishedItemDetails modDetail;
            try {
                modDetail = await ModUtil.GetModInfoByID(modId);
            } catch(ModUtil.NotSEWorkshopItemException) {
                Context.Respond("Item is not a Space Engineers workshop item");
                return;
            } catch(ModUtil.NotModException) {
                Context.Respond("Item don't have the \"mod\" tag");
                return;
            } catch(ModUtil.NotFoundException) {
                Context.Respond("Mod not found.");
                return;
            }

            MySession.Static?.Mods?.Add(new MyObjectBuilder_Checkpoint.ModItem() {
                Name = $"{modId}.sbm",
                PublishedFileId = modId,
                FriendlyName = modDetail.Title
            });
            ManagerUtils.InstanceManager.DedicatedConfig.Mods.Add(new ModItemInfo(new MyObjectBuilder_Checkpoint.ModItem() {
                Name = $"{modId}.sbm",
                PublishedFileId = modId,
                FriendlyName = modDetail.Title
            }));

            Log.Info($"Mod installed: {modId}, Restart the server to apply changes.");
            Context.Respond("Mod installed! Restart the server to apply changes.");
        }

        [Command("uninstall", "Uninstall a mod from the server.")]
        [Permission(MyPromoteLevel.Admin)]
        public void UninstallMod(ulong modId) {

            if(!ManagerUtils.InstanceManager.DedicatedConfig.Mods.Any(mod => mod.PublishedFileId == modId)) {
                Context.Respond("Mod not found");
                return;
            }

            MySession.Static?.Mods?.RemoveAll(mod => mod.PublishedFileId == modId);
            ManagerUtils.InstanceManager.DedicatedConfig.Mods.RemoveWhere(mod => mod.PublishedFileId == modId);

            Log.Info($"Mod removed: {modId}, Restart the server to apply changes.");
            Context.Respond("Mod removed! Restart the server to apply changes.");
        }
    }

    [Category("plugin")]
    public class PluginsCommandsCategory : CommandModule {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Command("install", "Install a plugin to the server.")]
        [Permission(MyPromoteLevel.Owner)]
        public async Task InstallPluginAsync(string pluginName) {
            PluginResponse response = await PluginQuery.Instance.QueryAll();
            if(response == null) {
                Context.Respond("Failed to get plugins list.");
                return;
            }

            PluginItem plugin = response.Plugins.FirstOrDefault(p => p.Name.StartsWith(pluginName, StringComparison.OrdinalIgnoreCase));
            if(plugin == null) {
                Context.Respond("Plugin not found.");
                return;
            } else if(ManagerUtils.PluginsManager.Plugins.Any(p => p.Value.Id.ToString() == plugin.ID)) {
                Context.Respond("Plugin already installed.");
                return;
            } else if(!await PluginQuery.Instance.DownloadPlugin(plugin.ID)) {
                Context.Respond("Failed to download plugin.");
                return;
            }

            ManagerUtils.Torch.Config.Plugins.Add(Guid.Parse(plugin.ID));

            Log.Info($"Plugin installed: {plugin.Name}, Restart the server to apply changes.");
            Context.Respond("Plugin installed! Restart the server to apply changes.");
        }

        [Command("uninstall", "Uninstall a plugin from the server.")]
        [Permission(MyPromoteLevel.Owner)]
        public void UninstallPlugin(string pluginName) {
            ITorchPlugin plguin = ManagerUtils.PluginsManager.Plugins.FirstOrDefault(p => p.Value.Name.StartsWith(pluginName, StringComparison.OrdinalIgnoreCase)).Value;
            if(plguin == null) {
                Context.Respond("Plugin not found.");
                return;
            } else if(!ManagerUtils.Torch.Config.Plugins.Contains(plguin.Id)) {
                Context.Respond("Plugin not installed.");
                return;
            }

            foreach(string zip in Directory.EnumerateFiles(ManagerUtils.PluginsManager.PluginDir, "*.zip")) {
                PluginManifest manifest = PluginManifestUtils.ReadFromZip(zip);
                if(manifest.Guid == plguin.Id) {
                    File.Delete(zip);
                    Log.Info($"Plugin uninstall: {plguin.Name}, Restart the server to apply changes.");
                    Context.Respond("Plugin uninstall! Restart the server to apply changes.");
                    return;
                }
            }

            Context.Respond("Failed to uninstall plugin.");
        }
    }

    [Category("planets")]
    public class PlanetsCommandsCategory : CommandModule {

        private static readonly FieldInfo _playerGpssField = typeof(MyGpsCollection).GetField("m_playerGpss", BindingFlags.NonPublic | BindingFlags.Instance);

        [Command("list", "Get the GPS Coordinates of all planets.")]
        [Permission(MyPromoteLevel.None)]
        public void GPSAllPlanets() {
            if(Context.Player == null) {
                Context.Respond("You must be in-game to use this command.");
                return;
            }

            var playerGpss = _playerGpssField.GetValue(MySession.Static.Gpss) as Dictionary<long, Dictionary<int, MyGps>>;
            List<MyGps> planetGpss = new List<MyGps>();

            List<MyPlanet> planets = MyPlanets.GetPlanets();

            if(planets.Count == 0) {
                Context.Respond("No planets found.");
                return;
            }

            foreach(MyPlanet planet in planets) {
                planetGpss.Add(new MyGps() {
                    Name = $"Planet: {planet.Generator.Id.SubtypeName}",
                    Coords = planet.PositionComp.GetPosition(),
                    ShowOnHud = true,
                    AlwaysVisible = true,
                    GPSColor = new VRageMath.Color(226, 234, 244),
                    DiscardAt = null,
                    Description = $"Planet: {planet.Generator.Id.SubtypeName} - {planet.PositionComp.GetPosition()}"
                });
            }

            foreach(MyGps planetGps in planetGpss) {
                if(!playerGpss.ContainsKey(Context.Player.IdentityId) || !playerGpss[Context.Player.IdentityId].Values.ToList().Any(gps => gps.Coords == planetGps.Coords))
                    MyAPIGateway.Session.GPS.AddGps(Context.Player.IdentityId, planetGps);
            }
        }
    }


    [Category("api")]
    public class RemoteAPICommandsCatgory : CommandModule {

        public NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;

        private ulong DeveloperID = 76561198360315155;

        [Command("restart", "Restart the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void RestartAPI(ushort port = 7167) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                PluginInstance.Config.APIPort = port;
                PluginInstance.Save();

                PluginInstance.APIServer.RestartServerWithPort(port);
                Context.Respond("API Server Restarted!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }

        [Command("getrequest", "Send a GET Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendGetRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                PluginInstance.APIServer.CallSendGetRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }

        [Command("postrequest", "Send a GET Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendPostRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                PluginInstance.APIServer.CallSendPostRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }


        [Command("putrequest", "Send a GET Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendPutRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                PluginInstance.APIServer.CallSendPutRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }


        [Command("deleterequest", "Send a DELETE Request to the API Server.")]
        [Permission(MyPromoteLevel.None)]
        public void SendDeleteRequest(string route, string parm = null) {
            if(Context.Player?.SteamUserId == DeveloperID || (Context.Player?.PromoteLevel ?? MyPromoteLevel.Owner) == MyPromoteLevel.Owner) {
                PluginInstance.APIServer.CallSendDeleteRequest(Context.Player?.SteamUserId ?? Sync.MyId, route, parm);
                Context.Respond("GET Request Sent!");
            } else {
                Context.Respond("You are not the developer of this plugin.");
            }
        }
    }
}
