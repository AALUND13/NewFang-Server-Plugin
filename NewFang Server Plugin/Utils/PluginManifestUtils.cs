using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Torch;

namespace NewFangServerPlugin {
    /// <summary>
    /// This class is from
    /// <see href="https://github.com/PveTeam/TorchRemote/blob/83a7e4ce24238a008af57e5f5c483034b552a1d0/TorchRemote.Plugin/Utils/PluginManifestUtils.cs"/>
    /// </summary>
    public class PluginManifestUtils {
        private static readonly XmlSerializer Serializer = new(typeof(PluginManifest));

        public static PluginManifest Read(Stream stream) {
            return (PluginManifest)Serializer.Deserialize(stream);
        }

        public static PluginManifest ReadFromZip(Stream stream) {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            using var entryStream = archive.GetEntry("manifest.xml")!.Open();
            return Read(entryStream);
        }

        public static PluginManifest ReadFromZip(string archivePath) {
            using var stream = File.OpenRead(archivePath);
            return ReadFromZip(stream);
        }
    }
}
