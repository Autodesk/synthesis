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
        [ExposedApi]
        public static void RegisterAssetType<TAsset>(string assetType, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            RegisterAssetTypeImpl<TAsset>(assetType, fileExtensions, handler);
        }

        internal static void RegisterAssetTypeImpl<TAsset>(string assetType, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
        {
            InnerInstance.RegisterAssetType<TAsset>(assetType, fileExtensions, handler);
        }

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="handler"></param>
        [ExposedApi]
        public static void RegisterAssetType<TAsset>(string type, string subtype, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            RegisterAssetTypeImpl<TAsset>(type, subtype, fileExtensions, handler);
        }

        internal static void RegisterAssetTypeImpl<TAsset>(string type, string subtype, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
        {
            InnerInstance.RegisterAsset<TAsset>(type, subtype, fileExtensions, handler);
        }

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        [ExposedApi]
        public static Asset? GetAsset(string targetPath)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return GetAssetImpl(targetPath);
        }

        internal static Asset? GetAssetImpl(string targetPath)
        {
            return InnerInstance.GetAsset(targetPath);
        }

        /// <summary>
        /// Fetch an Asset from the virtual file system
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TAsset? GetAsset<TAsset>(string targetPath) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return GetAssetImpl<TAsset>(targetPath);
        }

        internal static TAsset? GetAssetImpl<TAsset>(string targetPath) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.GetAsset(targetPath);
        }

        /// <summary>
        /// Recursively search the virtual file system for an asset with a given name
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TAsset? Search<TAsset>(string name) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return SearchImpl<TAsset>(name);
        }

        internal static TAsset? SearchImpl<TAsset>(string name) where TAsset : Asset
        {
            return FileSystem.SearchImpl<TAsset>(name);
        }

        /// <summary>
        /// Recursively search a directory for an asset with a given name
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TAsset? Search<TAsset>(VirtualFileSystem.Directory parent, string name) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return SearchImpl<TAsset>(parent, name);
        }

        internal static TAsset? SearchImpl<TAsset>(VirtualFileSystem.Directory parent, string name) where TAsset : Asset
        {
            return FileSystem.SearchImpl<TAsset>(parent, name);
        }

        /// <summary>
        /// Recursively search the virtual file system for an asset with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static Asset? Search(string name)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return SearchImpl(name);
        }

        internal static Asset? SearchImpl(string name)
        {
            return (Asset?)FileSystem.SearchImpl(name);
        }

        /// <summary>
        /// Recursively search a directory for an asset with a given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static Asset? Search(VirtualFileSystem.Directory parent, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return SearchImpl(parent, name);
        }

        internal static Asset? SearchImpl(VirtualFileSystem.Directory parent, string name)
        {
            return (Asset?)FileSystem.SearchImpl(parent, name);
        }

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
        [ExposedApi]
        public static Asset? ImportOrCreate(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportOrCreateImpl(assetType, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportOrCreateImpl(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, true, null, targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static TAsset? ImportOrCreate<TAsset>(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportOrCreateImpl<TAsset>(assetType, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static TAsset? ImportOrCreateImpl<TAsset>(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.Import(assetType, true, null, targetPath, name, owner, perm, sourcePath, args);
        }

        [ExposedApi]
        public static Asset? Import(string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl(targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportImpl(string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(false, null, targetPath, name, owner, perm, sourcePath, args);
        }

        [ExposedApi]
        public static TAsset? Import<TAsset>(string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl<TAsset>(targetPath, name, owner, perm, sourcePath, args);
        }

        internal static TAsset? ImportImpl<TAsset>(string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return InnerInstance.Import<TAsset>(false, null, targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static Asset? Import(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl(assetType, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportImpl(string assetType, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, false, null, targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static TAsset? Import<TAsset>(string assetType, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl<TAsset>(assetType, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static TAsset? ImportImpl<TAsset>(string assetType, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.Import(assetType, false, null, targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static Asset? Import(string assetType, byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl(assetType, data, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportImpl(string assetType, byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, false, data, targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static Asset? Import(string assetType, Stream stream, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl(assetType, stream, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportImpl(string assetType, Stream stream, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, false, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()),
                targetPath, name, owner, perm, sourcePath, args);
        }

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
        [ExposedApi]
        public static TAsset? Import<TAsset>(string assetType, byte[] data, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl<TAsset>(assetType, data, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static TAsset? ImportImpl<TAsset>(string assetType, byte[] data, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.Import(assetType, false, data, targetPath, name, owner, perm, sourcePath, args);
        }


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
        [ExposedApi]
        public static TAsset? Import<TAsset>(string assetType, Stream stream, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl<TAsset>(assetType, stream, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static TAsset? ImportImpl<TAsset>(string assetType, Stream stream, string targetPath, string name,
            Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.Import(assetType, false,
                Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd()), targetPath, name, owner, perm, sourcePath,
                args);
        }

        [ExposedApi]
        public static Asset? Import(Stream stream, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportImpl(stream, targetPath, name, owner, perm, sourcePath, args);
        }

        internal static Asset? ImportImpl(Stream stream, string targetPath, string name, Guid owner,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(false, Encoding.UTF8.GetBytes((new StreamReader(stream)).ReadToEnd()),
               targetPath, name, owner, perm, sourcePath, args);
        }

        /// <summary>
        /// Implementation of the public API of the AssetManager
        /// </summary>
        private class Inner
        {
            private Inner()
            {
                AssetHandlers = new Dictionary<string, Dictionary<string, HandlerFunc>>();
                FileExtensionAssetTypes = new Dictionary<string, (string, string)>();
                TypeAssetTypes = new Dictionary<Type, (string, string)>();

                RegisterAssetType<TextAsset>("text/plain", new string[]{".txt"},
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

                RegisterAssetType<XmlAsset>("text/xml", new string[] { ".xml" },
                    (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                        dynamic[] args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/xml asset: wrong number of arguments");
                        XmlAsset newAsset = new XmlAsset(name, owner, perm, sourcePath);
                        return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                    });

                RegisterAssetType<JsonAsset>("text/json", new string[] { ".json" },
                    (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                        dynamic[] args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/json asset: wrong number of arguments");
                        JsonAsset newAsset = new JsonAsset(name, owner, perm, sourcePath);
                        return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                    });

                RegisterAssetType<CssAsset>("text/css", new string[] { ".css" },
                   (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                       dynamic[] args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of text/css asset: wrong number of arguments");
                       CssAsset newAsset = new CssAsset(name, owner, perm, sourcePath);
                       return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                   });

                RegisterAssetType<SpriteAsset>("image/sprite", new string[] { ".png", ".jpeg" },
                   (byte[] data, string targetPath, string name, Guid owner, Permissions perm, string sourcePath,
                       dynamic[] args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of image/sprite asset: wrong number of arguments");
                       SpriteAsset newAsset = new SpriteAsset(name, owner, perm, sourcePath);
                       return (Asset?)FileSystem.AddResource(targetPath, newAsset.Load(data));
                   });
            }
            
            public Dictionary<string, Dictionary<string, HandlerFunc>> AssetHandlers { get; private set; }
            public Dictionary<string, (string, string)> FileExtensionAssetTypes { get; private set; }
            public Dictionary<Type, (string, string)> TypeAssetTypes { get; private set; }

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

            public void RegisterAssetType<TAsset>(string assetType, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
            {
                string[] types = SplitAssetType(assetType);
                RegisterAsset<TAsset>(types[0], types[1], fileExtensions, handler);
            }

            public void RegisterAsset<TAsset>(string type, string subtype, string[] fileExtensions, HandlerFunc handler) where TAsset : Asset
            {
                if(AssetHandlers.ContainsKey(type) && AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception($"Registering duplicate handler for {type}/{subtype}");
                }

                if (!AssetHandlers.ContainsKey(type))
                {
                    AssetHandlers[type] = new Dictionary<string, HandlerFunc>();
                }

                foreach (var fileExtension in fileExtensions) {
                    FileExtensionAssetTypes[fileExtension] = (type, subtype);
                }

                TypeAssetTypes[typeof(TAsset)] = (type, subtype);

                AssetHandlers[type][subtype] = handler;
            }

            public Asset? GetAsset(string targetPath) => (Asset?)FileSystem.TraverseImpl(targetPath);

            public Asset? Import(bool createOnFail, byte[]? data, string targetPath, string name,
                Guid owner, Permissions perm, string sourcePath, params dynamic[] args)
            {
                var types = FileExtensionAssetTypes[Path.GetExtension(sourcePath)];

                return Import(types.Item1, types.Item2, createOnFail, data, targetPath, name, owner, perm, sourcePath, args);
            }

            public TAsset? Import<TAsset>(bool createOnFail, byte[]? data, string targetPath, string name,
                Guid owner, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
            {
                var types = TypeAssetTypes[typeof(TAsset)];

                var types2 = FileExtensionAssetTypes[Path.GetExtension(sourcePath)];
                if (types.Item1 != types2.Item1 || types.Item2 != types2.Item2)
                {
                    // TODO error message
                }

                return (TAsset?)Import(types.Item1, types.Item2, createOnFail, data, targetPath, name, owner, perm, sourcePath, args);
            }

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

                var types = FileExtensionAssetTypes[Path.GetExtension(sourcePath)];
                if(type != types.Item1 || subtype != types.Item2)
                {
                    // TODO error message
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

                using var _ = ApiCallSource.IsInternal ? ApiCallSource.ForceInternalCall() : null; // TODO make this safe

                return AssetHandlers[type][subtype](data ?? new byte[0], targetPath, name, owner, perm, path, args);
            }

            public static readonly Inner InnerInstance = new Inner();
        }
        private static Inner InnerInstance => Inner.InnerInstance;
    }
}