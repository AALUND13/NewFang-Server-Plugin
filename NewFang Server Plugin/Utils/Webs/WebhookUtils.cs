using NLog;
using System.Net;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace NewFangServerPlugin.Utils.Webs {
    public class WebhookMessage {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarURL { get; set; }

        [JsonProperty("embeds")]
        public WebhookEmbed[] Embeds { get; set; }
    }

    public struct WebhookEmbed {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("fields")]
        public WebhookField[] Fields { get; set; }

        [JsonProperty("thumbnail")]
        public WebhookThumbnail Thumbnail { get; set; }

        [JsonProperty("image")]
        public WebhookImage Image { get; set; }

        [JsonProperty("footer")]
        public WebhookFooter Footer { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public struct WebhookFooter {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_url")]
        public string IconURL { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconURL { get; set; }
    }

    public struct WebhookImage {
        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("proxy_url")]
        public string ProxyURL { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public struct WebhookThumbnail {
        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("proxy_url")]
        public string ProxyURL { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    public struct WebhookField {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }

    public class WebhookUtils {
        private static Logger Log => NewFangServerPlugin.Log;

        public static async Task SendWebhookAsync(string url, WebhookMessage message) {
            if(string.IsNullOrWhiteSpace(url) || message == null) return;

            using(var client = new WebClient()) {
                client.Headers.Add("Content-Type", "application/json");

                try {
                    var settings = new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore, // Ignore null values
                        Formatting = Formatting.None // Optional: Set to Formatting.Indented for pretty-printing
                    };

                    string json = JsonConvert.SerializeObject(message, settings);
                    string response = await client.UploadStringTaskAsync(url, "POST", json);
                } catch(WebException ex) {
                    using(var reader = new System.IO.StreamReader(ex.Response.GetResponseStream())) {
                        string error = reader.ReadToEnd();
                        Log.Error($"Error sending webhook: {error}");
                    }
                } catch(Exception ex) {
                    Log.Error($"Unexpected error: {ex.Message}");
                }
            }
        }

        public static void SendWebhook(string url, WebhookMessage message) {
            if(string.IsNullOrWhiteSpace(url) || message == null) return;

            using(var client = new WebClient()) {
                client.Headers.Add("Content-Type", "application/json");

                try {
                    var settings = new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore, // Ignore null values
                        Formatting = Formatting.None // Optional: Set to Formatting.Indented for pretty-printing
                    };

                    string json = JsonConvert.SerializeObject(message, settings);
                    string response = client.UploadString(url, "POST", json);;
                } catch(WebException ex) {
                    using(var reader = new System.IO.StreamReader(ex.Response.GetResponseStream())) {
                        string error = reader.ReadToEnd();
                        Log.Error($"Error sending webhook: {error}");
                    }
                } catch(Exception ex) {
                    Log.Error($"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
