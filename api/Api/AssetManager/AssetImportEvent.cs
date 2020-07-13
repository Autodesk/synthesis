using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager
{
    public class AssetImportEvent : EventBus.IEvent
    {
        public const string Tag = "AssetImport";

        public string AssetName { get; }
        public string AssetLocation { get; }
        public string AssetType { get; }

        public AssetImportEvent(string assetName, string assetLocation, string assetType)
        {
            AssetName = assetName;
            AssetLocation = assetLocation;
            AssetType = assetType;
        }

        public object[] GetArguments() => new[] { AssetName, AssetLocation, AssetType };
    }
}
