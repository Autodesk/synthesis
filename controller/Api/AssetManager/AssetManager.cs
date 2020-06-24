using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;

#nullable enable

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// API for managing asset types and importing and fetching loaded assets
    /// </summary>
    public static class AssetManager
    {
        // TODO add dummy structs so we can do stuff like AssetManager.AssetManager.ImportOrCreate<Text,Json>(ActualFilePath);

        /// <summary>
        /// Asset-type-specific function delegate used to process asset data on import
        /// </summary>
        /// <param name="source_path"></param>
        /// <param name="data"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Asset? HandlerFunc(byte[] data, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args);

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
        /// <param name="target_path"></param>
        /// <returns></returns>
        public static Asset? GetAsset(string target_path) =>
            Instance.GetAsset(target_path);

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="target_path"></param>
        /// <returns></returns>
        public static TAsset? GetAsset<TAsset>(string target_path) where TAsset : Asset =>
            (TAsset?) Instance.GetAsset(target_path);

        /// <summary>
        /// Recursively search the virtual file system for an asset with a given name
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TAsset? Search<TAsset>(string name) where TAsset : Asset =>
            FileSystem.Search<TAsset>(name);

        /// <summary>
        /// Recursively search a directory for an asset with a given name
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TAsset? Search<TAsset>(VirtualFileSystem.Directory parent, string name) where TAsset : Asset => 
            FileSystem.Search<TAsset>(parent, name);

        /// <summary>
        /// Recursively search the virtual file system for an asset with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Asset? Search(string name) =>
            (Asset?)FileSystem.Search(name);

        /// <summary>
        /// Recursively search a directory for an asset with a given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Asset? Search(VirtualFileSystem.Directory parent, string name) =>
            (Asset?)FileSystem.Search(parent, name);

        public static Asset? ImportOrCreate(string asset_type, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) =>
           Instance.Import(asset_type, true, null, target_path, name, owner, perm, source_path, args);

        public static TAsset? ImportOrCreate<TAsset>(string asset_type, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) where TAsset : Asset =>
            (TAsset?)Instance.Import(asset_type, true, null, target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="source_path"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Asset? Import(string asset_type, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) =>
            Instance.Import(asset_type, false, null, target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="asset_type"></param>
        /// <param name="source_path"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string asset_type, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) where TAsset : Asset =>
            (TAsset)Instance.Import(asset_type, false, null, target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="data"></param>
        /// <param name="source_path"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Asset? Import(string asset_type, byte[] data, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) =>
            Instance.Import(asset_type, false, data, target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="asset_type"></param>
        /// <param name="source_path"></param>
        /// <param name="stream"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Asset? Import(string asset_type, Stream stream, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) =>
            Instance.Import(asset_type, false, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()), target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="asset_type"></param>
        /// <param name="source_path"></param>
        /// <param name="data"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string asset_type, byte[] data, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) where TAsset : Asset =>
            (TAsset?)Instance.Import(asset_type, false, data, target_path, name, owner, perm, source_path, args);


        // TODO: Remove assetType
        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="asset_type"></param>
        /// <param name="source_path"></param>
        /// <param name="stream"></param>
        /// <param name="target_path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string asset_type, Stream stream, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args) where TAsset : Asset =>
            (TAsset?)Instance.Import(asset_type, false, Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd()), target_path, name, owner, perm, source_path, args);

        /// <summary>
        /// Implementation of the public API of the AssetManager
        /// </summary>
        private class Inner
        {
            public Inner()
            {
                AssetHandlers = new Dictionary<string, Dictionary<string, HandlerFunc>>();

                RegisterAssetType("text/plain", (byte[] data, string target_path, string name, Guid owner, Permissions perms, string source_path, dynamic[] args) =>
                {
                    if (args.Length != 0)
                    {
                        throw new Exception();
                    }

                    TextAsset new_asset = new TextAsset(name, owner, perms, source_path);
                    return (Asset?)FileSystem.AddResource(target_path, new_asset.Load(data));
                });

                RegisterAssetType("text/xml", (byte[] data, string target_path, string name, Guid owner, Permissions perm, string source_path, dynamic[] args) =>
                {
                    if (args.Length != 0)
                    {
                        throw new Exception();
                    }

                    XMLAsset new_asset = new XMLAsset(name, owner, perm, source_path);
                    return (Asset?)FileSystem.AddResource(target_path, new_asset.Load(data));
                });

                RegisterAssetType("text/json", (byte[] data, string target_path, string name, Guid owner, Permissions perm, string source_path, dynamic[] args) =>
                {
                    if (args.Length != 0)
                    {
                        throw new Exception();
                    }
                    JSONAsset new_asset = new JSONAsset(name, owner, perm, source_path);
                    return (Asset?)FileSystem.AddResource(target_path, new_asset.Load(data));
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

            public Asset? GetAsset(string target_path)
            {
                return (Asset?)FileSystem.Traverse(target_path);
            }

            
            // TODO: Add overload that uses placeholder structs to replace asset_type
            // i.e. AssetManager.Import<Text,JSON>(...);

            public Asset? Import(string asset_type, bool create_on_fail, byte[]? data, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args)
            {
                var types = SplitAssetType(asset_type);

                return Import(types[0], types[1], create_on_fail, data, target_path, name, owner, perm, source_path, args);
            }

            public Asset? Import(string type, string subtype, bool create_on_fail, byte[]? data, string target_path, string name, Guid owner, Permissions perm, string source_path, params dynamic[] args)
            {
                if (!AssetHandlers.ContainsKey(type) || !AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception();
                }

                string path = FileSystem.BasePath + source_path;

                if (!File.Exists(path))
                {
                    if (create_on_fail)
                    {
                        File.Create(path);
                        data = new byte[0];
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (data == null)
                {
                    data = File.ReadAllBytes(path);
                }

                return AssetHandlers[type][subtype](data, target_path, name, owner, perm, source_path, args);
            }

            internal static readonly Inner instance = new Inner();
        }
        private static Inner Instance { get { return Inner.instance; } }
    }
}