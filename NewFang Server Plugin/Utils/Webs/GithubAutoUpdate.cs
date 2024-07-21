using NewFangServerPlugin.Handler;
using NewFangServerPlugin.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace NewFangServerPlugin.API {
    public class GithubAutoUpdate {
        public bool AutoUpdateEnabled { get; set; } = true;

        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        private string _manifestURL;
        private string _pluginURL;

        private string _pluginsPath;

        private Timer _updateTimer;
        private RestartTimer _restartTimer;

        public GithubAutoUpdate(string repository, string pluginName, int updateInterval) {
            _manifestURL = $"https://raw.githubusercontent.com/{repository}/master/manifest.json";
            _pluginURL = $"https://raw.githubusercontent.com/{repository}/master/Build/{pluginName}.zip";

            _pluginsPath = Path.Combine(PluginInstance.StoragePath.TrimEnd("Instance".ToCharArray()), "Plugins");

            // Immediately check for updates when class first initialized.
            CheckForUpdates();

            _updateTimer = new Timer(updateInterval);
            _updateTimer.Elapsed += (sender, e) => CheckForUpdates();
        }   

        private void CheckForUpdates() {
            if(!AutoUpdateEnabled || _restartTimer != null) return;

            Log.Info("Checking for updates...");

            string currentVersion = PluginInstance.Version.ToString();
            string latestVersion = GetVersionFromManifest();

            if (latestVersion != null && currentVersion != latestVersion) {
                Log.Info($"Update available! Current version: {currentVersion}, Latest version: {latestVersion}");
                Log.Info("Downloading update...");

                ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin is updating to version {latestVersion}! Please wait...");
                bool success = downloadLatestVersionPlugin(_pluginsPath, $"{PluginInstance.Name}.dll");

                if (success) {
                    Log.Info("Plugin Downloaded Successfully!");
                    ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin updated to version {latestVersion}! Restarting server...");
                    // Restart the server after 5 minutes
                    // If the server is not running, restart immediately
                    if(!ManagerUtils.Torch.IsRunning) 
                        PluginInstance.Torch.Restart();
                    else
                        _restartTimer = new RestartTimer(60 * 5);
                } else {
                    Log.Error("Failed to download the ZIP file.");
                    ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin failed to update!");
                }

            } else {
                Log.Info("No updates available.");
            }
        }

        private bool downloadLatestVersionPlugin(string path, string fileName) {
            string savePath = Path.Combine(path, fileName);

            try {
                using(WebClient webClient = new WebClient()) {
                    webClient.DownloadFile(_pluginURL, savePath);
                    Log.Info("ZIP file downloaded successfully.");
                }
                return true;
            } catch(Exception ex) {
                Log.Error($"Failed to download the ZIP file: {ex.Message}");
                return false;
            }
        }

        private string GetVersionFromManifest() {
            string version = null;

            using(var client = new System.Net.WebClient()) {
                try {
                    string xmlString = client.DownloadString(_manifestURL);

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xmlString);

                    XmlNode versionNode = xmlDocument.SelectSingleNode("/PluginManifest/Version");
                    version = versionNode.InnerText;
                } catch(Exception e) {
                    Log.Error(e, "Failed to get version from manifest!");
                }
            }

            return version;
        }
    }
}
