using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewFangServerPlugin.WebAPI.Models {
    public struct PluginInfoResponse {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Guid { get; set; }
    }
}
