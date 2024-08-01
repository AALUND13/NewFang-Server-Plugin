using NewFangServerPlugin;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Planet;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace NewFangServerPlugin {
    [Category("Planets")]
    public class PlanetsCommandCategory : CommandModule {

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

            foreach (MyGps planetGps in planetGpss)
            {
                if(!playerGpss.ContainsKey(Context.Player.IdentityId) || !playerGpss[Context.Player.IdentityId].Values.ToList().Any(gps => gps.Coords == planetGps.Coords))
                    MyAPIGateway.Session.GPS.AddGps(Context.Player.IdentityId, planetGps);
            }
        }
    }       


    [Category("RemoteAPI")]
    public class RemoteAPICommandCatgory : CommandModule {

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
