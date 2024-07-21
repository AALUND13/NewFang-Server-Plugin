using Torch;
using Torch.API.Managers;
using Torch.Managers;
using Torch.Server;
using Torch.Server.Managers;

namespace NewFangServerPlugin.Utils {
    public static class ManagerUtils {
        public static TorchServer Torch => (TorchServer)TorchBase.Instance;

        public static IChatManagerServer ChatManagerServer => Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>();
        public static InstanceManager InstanceManager => Torch.Managers?.GetManager<InstanceManager>();
        public static PluginManager PluginsManager => Torch.Managers?.GetManager<PluginManager>();
    }
}
