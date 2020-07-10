using System.IO;
using System.Xml;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.VirtualFileSystem;

namespace SynthesisAPI.AssetManager
{
    public class SynVisualElementAsset: Asset
    {
        private XmlDocument? _document;

        public SynVisualElementAsset(string name, Permissions perms, string sourcePath)
        {
            Init(name, perms, sourcePath);
        }

        public SynVisualElement GetElement(string name)
        {
            if (_document == null)
                Load(File.ReadAllBytes(FileSystem.BasePath + SourcePath));
            return UIParser.CreateVisualElement(name, _document);
        }

        public override IEntry Load(byte[] data)
        {
            _document = new XmlDocument();
            MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            _document.Load(stream);
            return this;
        }
    }
}