using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Torch.Utils.SteamWorkshopTools;

namespace NewFangServerPlugin.Utils {
    public static class ModUtil {
        private static NewFangServerPlugin PluginInstance => NewFangServerPlugin.Instance;
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public enum ErrorType {
            NotSEWorkshopItem,
            NotMod,
        }

        public async static Task<PublishedItemDetails> GetModInfoByID(ulong modID) {
            IEnumerable<ulong> modIDs = new List<ulong> { modID };
            Dictionary<ulong, PublishedItemDetails> steamWorkshopResponse = await Torch.Utils.SteamWorkshopTools.WebAPI.Instance.GetPublishedFileDetails(modIDs);
            

            if(steamWorkshopResponse == null || steamWorkshopResponse.Count == 0)
                throw new NotFoundException(modID);

            var itemDetails = steamWorkshopResponse.Values.First();
            //Log.Info($"Mod Details: {JsonConvert.SerializeObject(steamWorkshopResponse)}");

            if(itemDetails.ConsumerAppId != 244850)
                throw new NotSEWorkshopItemException(modID);

            if(itemDetails.Tags.All(tag => tag != "mod"))
                throw new NotModException(modID);

            return itemDetails;
        }

        public class NotFoundException : Exception {
            public NotFoundException(ulong publishedFileId)
                : base($"The item with ID: {publishedFileId} was not found.") { }
        }

        public class NotSEWorkshopItemException : Exception {
            public NotSEWorkshopItemException(ulong publishedFileId)
                : base($"The item with ID: {publishedFileId} is not a Space Engineers workshop item.") { }
        }

        public class NotModException : Exception {
            public NotModException(ulong publishedFileId)
                : base($"The item with ID: {publishedFileId} is not a mod.") { }
        }
    }
}
