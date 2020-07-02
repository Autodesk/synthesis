using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager.DummyAssetTypes
{
    public interface IAssetType { }
    public interface IAssetSubtype { }

    // Text types
    public struct Text : IAssetType { }
    public struct Plain : IAssetSubtype { }
    public struct Xml : IAssetSubtype { }
    public struct Json : IAssetSubtype { }
    public struct Css : IAssetSubtype { }

    // Binary types
    public struct Binary : IAssetType { }

    // Image types
    public struct Image : IAssetType { }
    public struct Sprite : IAssetSubtype { }
}
