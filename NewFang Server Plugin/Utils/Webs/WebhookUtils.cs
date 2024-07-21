using System.Net;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using NLog;

namespace NewFangServerPlugin.Utils.Webs {

    public class WebhookMessage {
        [JsonPropertyName("content")] public string Content { get; set; }
        [JsonPropertyName("username")] public string Username { get; set; }
        [JsonPropertyName("avatar_url")] public string AvatarURL { get; set; }
    }

    public class WebhookUtils {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        private static Logger Log => NewFangServerPlugin.Log;

        public static void SendWebhook(string url, WebhookMessage message) {
            if(string.IsNullOrWhiteSpace(url) || message == null) return;

            using(var client = new WebClient()) {
                client.Headers.Add("Content-Type", "application/json");

                try {
                    var options = new JsonSerializerOptions {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null values
                        WriteIndented = false // Optional: Set to true for pretty-printing
                    };

                    string json = JsonSerializer.Serialize(message, options);
                    string response = client.UploadString(url, "POST", json);
                } catch(WebException ex) {
                    using(var reader = new System.IO.StreamReader(ex.Response.GetResponseStream())) {
                        string error = reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
