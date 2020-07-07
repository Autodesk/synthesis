using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using System.IO;

#nullable enable

namespace SynthesisAPI.AssetManager
{
    public class LazyAsset : Asset
    {
        private Asset inner;
        private Stream _sourceStream;
        private string _targetPath;

        public LazyAsset(Asset asset, Stream sourceStream, string targetPath)
        {
            Init(asset.Name, asset.Permissions, asset.SourcePath);
            inner = asset;
            _sourceStream = sourceStream;
            _targetPath = targetPath;
        }

        public IEntry Load()
        {
            base.DeleteInner();
            
            byte[] data = new byte[_sourceStream.Length];
            _sourceStream.Read(data, 0, (int)_sourceStream.Length);
            _sourceStream.Close();

            var entry = FileSystem.AddEntry(_targetPath, inner.Load(data));
            
            if (entry != null)
            {
                return entry;
            }

            throw new System.Exception("Failed to load lazy asset");
        }

        public override IEntry Load(byte[] _)
        {
            return this;
        }

        internal override void DeleteInner()
        {
            base.DeleteInner();
            _sourceStream.Close();
        }
    }
}
