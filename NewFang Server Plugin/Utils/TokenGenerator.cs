using System;
using System.Collections.Generic;
using System.Linq;

namespace NewFangServerPlugin.Utils {
    public static class TokenGenerator {
        private static char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        public static string GeneratedToken() {
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 64).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
