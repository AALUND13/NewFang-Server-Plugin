using NewFangServerPlugin.Handler;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Timers;
using System.Xml;

namespace NewFangServerPlugin.Utils.Webs {
    public class GithubAutoUpdate {
        public bool AutoUpdateEnabled = true;

        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        private string _manifestURL;
        private string _pluginURL;

        private string _pluginsPath;

        private Timer _updateTimer;

        public GithubAutoUpdate(string repository, string pluginName, int updateInterval) {
            _manifestURL = $"https://raw.githubusercontent.com/{repository}/master/manifest.xml";
            _pluginURL = $"https://raw.githubusercontent.com/{repository}/master/Build/{pluginName}.zip";

            _pluginsPath = Path.Combine(PluginInstance.StoragePath.TrimEnd("Instance".ToCharArray()), "Plugins");

            // Immediately check for updates when class first initialized.
            CheckForUpdates();

            _updateTimer = new Timer(updateInterval);
            _updateTimer.Elapsed += (sender, e) => CheckForUpdates();
            _updateTimer.Start();
        }

        private void CheckForUpdates() {
            if(!AutoUpdateEnabled || RestartTimer.CountDownTimer != null) return;

            string currentVersion = PluginInstance.Version.ToString();
            string latestVersion = GetVersionFromManifest();

            if(latestVersion != null && currentVersion != latestVersion) {
                Log.Info($"Update available! Current version: {currentVersion}, Latest version: {latestVersion}");
                Log.Info("Downloading update...");

                ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin is updating to version {latestVersion}! Please wait...");
                bool success = downloadLatestVersionPlugin(_pluginsPath, $"{PluginInstance.Name}.zip");

                if(success) {
                    Log.Info("Plugin Downloaded Successfully!");
                    ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin updated to version {latestVersion}! Restarting server...");

                    // Restart the server after 5 minutes
                    // If the server is not running, restart immediately
                    if(!ManagerUtils.Torch.IsRunning)
                        PluginInstance.Torch.Restart();
                    else
                        new RestartTimer(60 * 5);
                } else {
                    Log.Error("Failed to download the ZIP file.");
                    ManagerUtils.ChatManagerServer?.SendMessageAsSelf($"'{PluginInstance.Name}' plugin failed to update!");
                }
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

            using(var client = new WebClient()) {
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
