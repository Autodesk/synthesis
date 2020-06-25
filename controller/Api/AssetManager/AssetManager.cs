using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mime;
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
        // TODO do we want to add dummy structs so we can do stuff like AssetManager.ImportOrCreate<Text,Json>(ActualFilePath)

        /// <summary>
        /// Asset-type-specific function delegate used to process asset data on import
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetPath"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Asset? HandlerFunc(byte[] data, string targetPath, string name, Guid owner, Permissions perm,
            string sourcePath, params dynamic[] args);

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="handler"></param>
        public static void RegisterAssetType(string assetType, HandlerFunc handler) =>
            InnerInstance.RegisterAssetType(assetType, handler);

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="handler"></param>
        public static void RegisterAssetType(string type, string subtype, HandlerFunc handler) =>
            InnerInstance.RegisterAsset(type, subtype, handler);

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static Asset? GetAsset(string targetPath) =>
            InnerInstance.GetAsset(targetPath);

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static TAsset? GetAsset<TAsset>(string targetPath) where TAsset : Asset =>
            (TAsset?) InnerInstance.GetAsset(targetPath);

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

        /// <summary>
        /// Import a new asset into the virtual file system and create it if import fails
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="targetPath"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Asset? ImportOrCreate(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) =>
           InnerInstance.Import(assetType, true, null, targetPath, name, owner, perm, sourcePath, args);

        /// <summary>
        /// Import a new asset into the virtual file system and create it if import fails
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="assetType"></param>
        /// <param name="targetPath"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TAsset? ImportOrCreate<TAsset>(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset =>
            (TAsset?)InnerInstance.Import(assetType, true, null, targetPath, name, owner, perm, sourcePath, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static Asset? Import(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) =>
            InnerInstance.Import(assetType, false, null, targetPath, name, owner, perm, sourcePath, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="assetType"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string assetType, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset =>
            (TAsset)InnerInstance.Import(assetType, false, null, targetPath, name, owner, perm, sourcePath, args)!;

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="data"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static Asset? Import(string assetType, byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) =>
            InnerInstance.Import(assetType, false, data, targetPath, name, owner, perm, sourcePath, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="stream"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Asset? Import(string assetType, Stream stream, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args) =>
            InnerInstance.Import(assetType, false, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()),
            targetPath, name, owner, perm, sourcePath, args);

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="assetType"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="data"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string assetType, byte[] data, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset =>
            (TAsset?)InnerInstance.Import(assetType, false, data, targetPath, name, owner, perm, sourcePath, args);


        // TODO: Remove assetType
        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="assetType"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="stream"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static TAsset? Import<TAsset>(string assetType, Stream stream, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset =>
            (TAsset?) InnerInstance.Import(assetType, false,
            Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd()), targetPath, name, owner, perm, sourcePath,
            args);

        /// <summary>
        /// Implementation of the public API of the AssetManager
        /// </summary>
        private class Inner
        {
            private Inner()
            {
                AssetHandlers = new Dictionary<string, Dictionary<string, HandlerFunc>>();

                RegisterAssetType("text/plain",
                    (byte[] data, string targetPath, string name, Guid owner, Permissions perms, string sourcePath,
                        dynamic[] args) =>
                {
                    if (args.Length != 0)
                    {
                        throw new Exception("Import of text/plain asset: wrong number of arguments");
                    }

                    TextAsset newAsset = new TextAsset(name, owner, perms, sourcePath);
                    return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                });

                RegisterAssetType("text/xml",
                    (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                        dynamic[] args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/xml asset: wrong number of arguments");
                        XmlAsset newAsset = new XmlAsset(name, owner, perm, sourcePath);
                        return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                    });

                RegisterAssetType("text/json",
                    (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                        dynamic[] args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/json asset: wrong number of arguments");
                        JsonAsset newAsset = new JsonAsset(name, owner, perm, sourcePath);
                        return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                    });

                RegisterAssetType("text/css",
                   (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                       dynamic[] args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of text/css asset: wrong number of arguments");
                       CssAsset newAsset = new CssAsset(name, owner, perm, sourcePath);
                       return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                   });

                RegisterAssetType("image/sprite",
                   (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                       dynamic[] args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of image/sprite asset: wrong number of arguments");
                       SpriteAsset newAsset = new SpriteAsset(name, owner, perm, sourcePath);
                       return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                   });
            }
            
            public Dictionary<string, Dictionary<string, HandlerFunc>> AssetHandlers;

            /// <summary>
            /// Split a media type into type and subtype
            /// 
            /// Example: "text/plain" => "test" "plain"
            /// </summary>
            /// <param name="assetType"></param>
            /// <returns></returns>
            private string[] SplitAssetType(string assetType)
            {
                var types = assetType.Split('/');
                if (types.Length != 2)
                {
                    throw new Exception("Splitting asset type: wrong type format");
                }
                return types;
            }

            public void RegisterAssetType(string assetType, HandlerFunc handler)
            {
                string[] types = SplitAssetType(assetType);
                RegisterAsset(types[0], types[1], handler);
            }

            public void RegisterAsset(string type, string subtype, HandlerFunc handler)
            {
                if(AssetHandlers.ContainsKey(type) && AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception($"Registering duplicate handler for {type}/{subtype}");
                }

                if (!AssetHandlers.ContainsKey(type))
                {
                    AssetHandlers[type] = new Dictionary<string, HandlerFunc>();
                }

                AssetHandlers[type][subtype] = handler;
            }

            public Asset? GetAsset(string targetPath) => (Asset?)FileSystem.Traverse(targetPath);


            // TODO: Add overload that uses placeholder structs to replace asset_type
            // i.e. AssetManager.Import<Text,JSON>(...);

            public Asset? Import(string assetType, bool createOnFail, byte[]? data, string targetPath, string name,
                Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
            {
                var types = SplitAssetType(assetType);

                return Import(types[0], types[1], createOnFail, data, targetPath, name, owner, perm, sourcePath, args);
            }

            public Asset? Import(string type, string subtype, bool createOnFail, byte[]? data, string targetPath,
                string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
            {
                if (!AssetHandlers.ContainsKey(type) || !AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception($"Importing asset with unregistered type {type}/{subtype}");
                }

                string path = FileSystem.BasePath + sourcePath;

                if (!File.Exists(path))
                {
                    if (createOnFail)
                    {
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.Create(path).Close();
                        data = new byte[0];
                    }
                }
                else
                {
                    data = File.ReadAllBytes(path);
                }

                return AssetHandlers[type][subtype](data ?? new byte[0], targetPath, name, owner, perm, path, args);
            }

            public static readonly Inner InnerInstance = new Inner();
        }
        private static Inner InnerInstance => Inner.InnerInstance;
    }
}