using System;
using System.Net.Sockets;

namespace NewFangServerPlugin.Utils.Webs {
    public static class PortChecker {
        public static bool IsPortInUse(int port) {
            using(TcpClient tcpClient = new TcpClient()) {
                try {
                    tcpClient.Connect("127.0.0.1", port);
                    return true;
                } catch(Exception) {
                    return false;
                }
            }
        }
    }
}
