using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Torch.Utils.SteamWorkshopTools;

namespace NewFangServerPlugin.Utils {
    public static class ModUtil {
        public enum ErrorType {
            NotSEWorkshopItem,
            NotMod,
        }
        public async static Task<PublishedItemDetails> GetModInfoByID(ulong modID) {
            IEnumerable<ulong> modIDs = new List<ulong> { modID };
            Dictionary<ulong, PublishedItemDetails> steamWorkshopRespone = await WebAPI.Instance.GetPublishedFileDetails(modIDs);

            if(steamWorkshopRespone[0].ConsumerAppId != 244850)
                throw new NotSEWorkshopItemException(modID);

            if(steamWorkshopRespone[0].Tags.All(tag => tag != "Mod"))
                throw new NotModException(modID);

            return steamWorkshopRespone[0];
        }

        public class NotSEWorkshopItemException : Exception {
            public NotSEWorkshopItemException(ulong publishedFileId) : base($"The item with ID: {publishedFileId} is not a Space Engineers workshop item.") { }
        }

        public class NotModException : Exception {
            public NotModException(ulong publishedFileId) : base($"The item with ID: {publishedFileId} is not a mod.") { }
        }
    }
}