using SynthesisAPI.VirtualFileSystem;
using System.IO;

namespace SynthesisAPI.AssetManager
{
    public class LazyAsset : Asset
    {
        private string _type;
        private Stream _sourceStream;
        private string _targetPath;

        public LazyAsset(string type, Stream sourceStream, string targetPath)
        {
            _type = type;
            _sourceStream = sourceStream;
            _targetPath = targetPath;
        }

        public IEntry Get()
        {
            DeleteInner();

            return AssetManager.ImportInner(_type, _sourceStream, _targetPath, Name, Permissions, SourcePath);
        }

        public override IEntry Load(byte[] _)
        {
            return this;
        }
    }
}
