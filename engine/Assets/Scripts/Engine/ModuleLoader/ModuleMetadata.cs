using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
namespace Engine.ModuleLoader
{
    public class ModuleMetadata
    {
        public ModuleMetadata() { }

        public ModuleMetadata(string name, string version, string targetPath, IEnumerable<string> dependencies = null,
            IEnumerable<string> manifest = null)
        {
            Name = name;
            Version = version;
            TargetPath = targetPath;
            Dependencies = dependencies?.ToList() ?? new List<string>();
            FileManifest = manifest?.ToList() ?? new List<string>();
        }

        public static string MetadataFilename = "metadata.xml";

        public static ModuleMetadata Deserialize(Stream stream)
        {
            return (ModuleMetadata) new XmlSerializer(typeof(ModuleMetadata)).Deserialize(new StreamReader(stream));
        }

        public void Serialize(Stream stream)
        {
            new XmlSerializer(typeof(ModuleMetadata)).Serialize(new StreamWriter(stream), this);
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string TargetPath { get; set; }

        public List<string> Dependencies { get; set; }

        public List<string> FileManifest { get; set; }
	}
}