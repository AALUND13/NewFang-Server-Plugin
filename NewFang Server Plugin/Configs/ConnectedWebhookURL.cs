using Torch;
using Torch.Views;

namespace NewFangServerPlugin.Configs {
    public class ConnectedWebhookURL : ViewModel {
        private string _webhookURL;

        [Display(Name = "Webhook URLs", Description = "The URL of the webhook to connect to.")]
        public string WebhookURL { get => _webhookURL; set => SetValue(ref _webhookURL, value); }

        public override string ToString() {
            return WebhookURL;
        }
    }
}
