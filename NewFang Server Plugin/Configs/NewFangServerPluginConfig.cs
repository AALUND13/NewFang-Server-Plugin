﻿using NewFangServerPlugin.Utils;
using Torch;
using Torch.Collections;
using Torch.Views;

namespace NewFangServerPlugin.Configs {
    public class NewFangServerPluginConfig : ViewModel {
        public NewFangServerPluginConfig() {
            ConnectedWebhookURLs.CollectionChanged += (sender, args) => OnPropertyChanged();
        }

        [Display(EditorType = typeof(EmbeddedCollectionEditor))]
        public MtObservableList<ConnectedWebhookURL> ConnectedWebhookURLs { get; } = new MtObservableList<ConnectedWebhookURL>();

        private string _apiKey = TokenGenerator.GeneratedToken();
        [Display(Name = "API Key", Description = "The API Key for NewFang.")]
        public string APIKey { get => _apiKey; set => SetValue(ref _apiKey, value); }

        private int _apiPort = 7167;
        [Display(Name = "API Port", Description = "The port for the API Server.")]
        public int APIPort { get => _apiPort; set => SetValue(ref _apiPort, value); }

        private string _urlPrefix = "http://209.236.114.243:7167/";
        [Display(Name = "URL Prefix", Description = "The URL prefix for the API Server.")]
        public string URLPrefix { get => _urlPrefix; set => SetValue(ref _urlPrefix, value); }
    }
}
