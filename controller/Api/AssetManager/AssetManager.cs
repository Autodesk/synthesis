using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SynthesisAPI.VirtualFileSystem;


namespace SynthesisAPI.AssetManager
{
    public static class AssetManager
    {
        public delegate IAsset HandlerFunc(string path, byte[] data, params dynamic[] args);

        public static void RegisterAssetType(string asset_type, HandlerFunc handler) =>
            Instance.RegisterAssetType(asset_type, handler);

        public static void RegisterAssetType(string type, string subtype, HandlerFunc handler) =>
            Instance.RegisterAsset(type, subtype, handler);

        public static IAsset GetAsset(string path) =>
            Instance.GetAsset(path);

        public static TAsset GetAsset<TAsset>(string path) where TAsset : class, IAsset =>
            (TAsset) Instance.GetAsset(path);

        public static IAsset Import(string asset_type, string path, byte[] data, params dynamic[] args) =>
            Instance.Import(asset_type, path, data, args);

        public static IAsset Import(string asset_type, string path, Stream stream, params dynamic[] args) =>
            Instance.Import(asset_type, path, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()), args);

        public static TAsset Import<TAsset>(string asset_type, string path, byte[] data, params dynamic[] args) where TAsset : IAsset =>
            (TAsset)Instance.Import(asset_type, path, data, args);

        public static TAsset Import<TAsset>(string asset_type, string path, Stream stream, params dynamic[] args) where TAsset : IAsset =>
            (TAsset)Instance.Import(asset_type, path, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()), args);

        private class Inner
        {
            public Inner()
            {
                AssetHandlers = new Dictionary<string, Dictionary<string, HandlerFunc>>();

                RegisterAssetType("text/plain", (string path, byte[] data, dynamic[] args) =>
                {
                    if (args.Length != 3)
                    {
                        throw new Exception();
                    }

                    PlainTextAsset new_asset = new PlainTextAsset(args[0], args[1], args[2]);
                    return (IAsset)new_asset.LoadAsset(path, data);
                });

                RegisterAssetType("text/xml", (string path, byte[] data, dynamic[] args) =>
                {
                    if (args.Length != 3)
                    {
                        throw new Exception();
                    }

                    XMLAsset new_asset = new XMLAsset(args[0], args[1], args[2]);
                    return (IAsset)new_asset.LoadAsset(path, data);
                });

                RegisterAssetType("text/json", (string path, byte[] data, dynamic[] args) =>
                {
                    if (args.Length != 3)
                    {
                        throw new Exception();
                    }

                    JSONAsset new_asset = new JSONAsset(args[0], args[1], args[2]);
                    return (IAsset)new_asset.LoadAsset(path, data);
                });
            }
            
            public Dictionary<string, Dictionary<string, HandlerFunc>> AssetHandlers;

            private string[] SplitAssetType(string asset_type)
            {
                var types = asset_type.Split('/');
                if (types.Length != 2)
                {
                    throw new Exception();
                }
                return types;
            }

            public void RegisterAssetType(string asset_type, HandlerFunc handler)
            {
                string[] types = SplitAssetType(asset_type);
                RegisterAsset(types[0], types[1], handler);
            }

            public void RegisterAsset(string type, string subtype, HandlerFunc handler)
            {
                if(AssetHandlers.ContainsKey(type) && AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception();
                }

                if (!AssetHandlers.ContainsKey(type))
                {
                    AssetHandlers[type] = new Dictionary<string, HandlerFunc>();
                }

                AssetHandlers[type][subtype] = handler;
            }

            public IAsset GetAsset(string path)
            {
                return (IAsset)FileSystem.Traverse(path);
            }

            public IAsset Import(string asset_type, string path, byte[] data, params dynamic[] args)
            {
                var types = SplitAssetType(asset_type);

                return Import(types[0], types[1], path, data, args);
            }

            public IAsset Import(string type, string subtype, string path, byte[] data, params dynamic[] args)
            {
                if (!AssetHandlers.ContainsKey(type) || !AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception();
                }

                return AssetHandlers[type][subtype](path, data, args);
            }

            internal static readonly Inner instance = new Inner();
        }
        private static Inner Instance { get { return Inner.instance; } }
    }
}