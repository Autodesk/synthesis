using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Engine.ModuleLoader {
    public class ModuleMetadata {
        public class Dependency {
            public string Name;
            public string Version;

            public Dependency() {
            }

            public Dependency(string name, string version) {
                Name    = name;
                Version = version;
            }
        }

        public ModuleMetadata() {
            Dependencies = new List<Dependency>();
            FileManifest = new List<string>();
        }

        public ModuleMetadata(string name, string version, string targetPath,
            IEnumerable<Dependency> dependencies = null, IEnumerable<string> manifest = null) {
            Name         = name;
            Version      = version;
            TargetPath   = targetPath;
            Dependencies = dependencies?.ToList() ?? new List<Dependency>();
            FileManifest = manifest?.ToList() ?? new List<string>();
        }

        public static string MetadataFilename = "metadata.xml";

        public static ModuleMetadata Deserialize(Stream stream) {
            return (ModuleMetadata) new XmlSerializer(typeof(ModuleMetadata)).Deserialize(new StreamReader(stream));
        }

        public void Serialize(Stream stream, bool keepPosition = true) {
            var pos = stream.Position;
            new XmlSerializer(typeof(ModuleMetadata)).Serialize(new StreamWriter(stream), this);
            if (keepPosition)
                stream.Position = pos;
        }

        public override string ToString() {
            var m = new MemoryStream();
            Serialize(m);
            return new StreamReader(m).ReadToEnd();
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string TargetPath { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }

        public List<Dependency> Dependencies { get; set; }

        public List<string> FileManifest { get; set; }
    }
}