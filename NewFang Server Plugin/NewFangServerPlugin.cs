using NewFangServerPlugin.API;
using NewFangServerPlugin.Configs;
using NewFangServerPlugin.Handler;
using NewFangServerPlugin.Utils.Webs;
using NLog;
using System;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;

namespace NewFangServerPlugin {
    public class NewFangServerPlugin : TorchPluginBase, IWpfPlugin {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "NewFangServerPluginConfig.cfg";

        private NewFangServerPluginControl _control;
        public UserControl GetControl() => _control ?? (_control = new NewFangServerPluginControl(this));

        private Persistent<NewFangServerPluginConfig> _config;

        public NewFangServerPluginConfig Config => _config?.Data;

        public static NewFangServerPlugin Instance { get; private set; }

        public APIServer APIServer;


        public override void Init(ITorchBase torch) {
            base.Init(torch);
            Instance = this;

            SetupConfig();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if(sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();

            // Setup the GitHub auto updater, That check for updates every 10 minutes on GitHub.
            new GithubAutoUpdate("AALUND13/NewFang-Server-Plugin", "NewFangServerPlugin", 60000 * 10);
            // Setup/Start the API Server.
            APIServer = new APIServer(Config.APIPort);
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state) {

            switch(state) {
                case TorchSessionState.Loading:
                    Log.Info("Session Loading!");

                    foreach(ConnectedWebhookURL url in Config.ConnectedWebhookURLs) {
                        WebhookUtils.SendWebhook(url.WebhookURL, new WebhookMessage() {
                            Content = "Server is starting!",
                            Username = "Server"
                        });
                    }
                    break;
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    PluginEventHandler.Load();

                    foreach(ConnectedWebhookURL url in Config.ConnectedWebhookURLs) {
                        WebhookUtils.SendWebhook(url.WebhookURL, new WebhookMessage() {
                            Content = "Server has started!",
                            Username = "Server"
                        });
                    }
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    PluginEventHandler.Unload();

                    foreach(ConnectedWebhookURL url in Config.ConnectedWebhookURLs) {
                        WebhookUtils.SendWebhook(url.WebhookURL, new WebhookMessage() {
                            Content = "Server is shutting down!",
                            Username = "Server"
                        });
                    }
                    break;
            }
        }

        private void SetupConfig() {

            var configFile = Path.Combine(StoragePath, CONFIG_FILE_NAME);

            try {

                _config = Persistent<NewFangServerPluginConfig>.Load(configFile);

            } catch(Exception e) {
                Log.Warn(e);
            }

            if(_config?.Data == null) {

                Log.Info("Create Default Config, because none was found!");

                _config = new Persistent<NewFangServerPluginConfig>(configFile, new NewFangServerPluginConfig());
                _config.Save();
            }
        }

        public void Save() {
            try {
                _config.Save();
                Log.Info("Configuration Saved.");
            } catch(IOException e) {
                Log.Warn(e, "Configuration failed to save");
            }
        }
    }
}
