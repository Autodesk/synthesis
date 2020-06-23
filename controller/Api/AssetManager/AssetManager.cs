using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SynthesisAPI.VirtualFileSystem;


namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// API for managing asset types and importing and fetching loaded assets
    /// </summary>
    public static class AssetManager
    {
        /// <summary>
        /// Asset-type-specific function delegate used to process asset data on import
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate IAsset HandlerFunc(string path, byte[] data, params dynamic[] args);

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="handler"></param>
        public static void RegisterAssetType(string asset_type, HandlerFunc handler) =>
            Instance.RegisterAssetType(asset_type, handler);

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="handler"></param>
        public static void RegisterAssetType(string type, string subtype, HandlerFunc handler) =>
            Instance.RegisterAsset(type, subtype, handler);

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IAsset GetAsset(string path) =>
            Instance.GetAsset(path);

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TAsset GetAsset<TAsset>(string path) where TAsset : class, IAsset =>
            (TAsset) Instance.GetAsset(path);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IAsset Import(string asset_type, string path, byte[] data, params dynamic[] args) =>
            Instance.Import(asset_type, path, data, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IAsset Import(string asset_type, string path, Stream stream, params dynamic[] args) =>
            Instance.Import(asset_type, path, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()), args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="asset_type"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset Import<TAsset>(string asset_type, string path, byte[] data, params dynamic[] args) where TAsset : IAsset =>
            (TAsset)Instance.Import(asset_type, path, data, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="asset_type"></param>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset Import<TAsset>(string asset_type, string path, Stream stream, params dynamic[] args) where TAsset : IAsset =>
            (TAsset)Instance.Import(asset_type, path, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()), args);

        /// <summary>
        /// Implementation of the public API of the AssetManager
        /// </summary>
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
                    return (IAsset)FileSystem.AddResource(path, new_asset.Load(data));
                });

                RegisterAssetType("text/xml", (string path, byte[] data, dynamic[] args) =>
                {
                    if (args.Length != 3)
                    {
                        throw new Exception();
                    }

                    XMLAsset new_asset = new XMLAsset(args[0], args[1], args[2]);
                    return (IAsset)FileSystem.AddResource(path, new_asset.Load(data));
                });

                RegisterAssetType("text/json", (string path, byte[] data, dynamic[] args) =>
                {
                    if (args.Length != 3)
                    {
                        throw new Exception();
                    }

                    JSONAsset new_asset = new JSONAsset(args[0], args[1], args[2]);
                    return (IAsset)FileSystem.AddResource(path, new_asset.Load(data));
                });
            }
            
            public Dictionary<string, Dictionary<string, HandlerFunc>> AssetHandlers;

            /// <summary>
            /// Split a media type into type and subtype
            /// 
            /// Example: "text/plain" => "test" "plain"
            /// </summary>
            /// <param name="asset_type"></param>
            /// <returns></returns>
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