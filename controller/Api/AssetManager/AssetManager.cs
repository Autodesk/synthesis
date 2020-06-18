using System;
using System.Linq;
using System.Collections.Generic;

namespace SynthesisAPI.AssetManager
{
    public sealed class AssetManager
    {
        private AssetManager() {
            AssetHandlers = new Dictionary<string, Dictionary<string, Func<dynamic[], IAsset>>>();
        }

        public static AssetManager Instance { get { return Inner.instance; } }

        private class Inner
        {
            internal static readonly AssetManager instance = new AssetManager();
        }

        private readonly Dictionary<string, Dictionary<string, Func<dynamic[], IAsset>>> AssetHandlers;

        public void RegisterAsset(string type, string subtype, Func<dynamic[], IAsset> handler)
        {
            if(AssetHandlers.ContainsKey(type))
            {
                throw new Exception();
            }

            if (AssetHandlers[type].ContainsKey(subtype))
            {
                throw new Exception();
            }

            AssetHandlers[type][subtype] = handler;
        }

        public static IAsset? GetAsset(string path)
        {
            return null;
        }

        public static TAsset? GetAsset<TAsset>(string path) where TAsset : class, IAsset
        {
            return null;
        }
    }
}