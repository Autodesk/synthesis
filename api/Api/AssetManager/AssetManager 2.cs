using System;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// Asset-type-specific function delegate used to process asset data on import
        /// </summary>
        /// <param name="name"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate Asset? HandlerFunc(string name, Permissions perm,
            string sourcePath, params dynamic[] args);

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="fileExtensions"></param>
        /// <param name="handler"></param>
        [ExposedApi]
        public static void RegisterAssetType(string assetType, string[] fileExtensions, HandlerFunc handler)
        {
            using var _ = ApiCallSource.StartExternalCall();
            RegisterAssetTypeInner(assetType, fileExtensions, handler);
        }

        internal static void RegisterAssetTypeInner(string assetType, string[] fileExtensions, HandlerFunc handler)
        {
            InnerInstance.RegisterAssetType(assetType, fileExtensions, handler);
        }

        /// <summary>
        /// Register a handler for importing a new type of asset
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="fileExtensions"></param>
        /// <param name="handler"></param>
        [ExposedApi]
        public static void RegisterAssetType(string type, string subtype, string[] fileExtensions, HandlerFunc handler)
        {
            using var _ = ApiCallSource.StartExternalCall();
            RegisterAssetTypeInner(type, subtype, fileExtensions, handler);
        }

        internal static void RegisterAssetTypeInner(string type, string subtype, string[] fileExtensions, HandlerFunc handler)
        {
            InnerInstance.RegisterAsset(type, subtype, fileExtensions, handler);
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
            return GetAssetInner(targetPath);
        }

        internal static Asset? GetAssetInner(string targetPath)
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
            return GetAssetInner<TAsset>(targetPath);
        }

        internal static TAsset? GetAssetInner<TAsset>(string targetPath) where TAsset : Asset
        {
            return (TAsset?)InnerInstance.GetAsset(targetPath);
        }

        #region Search

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
            return SearchInner<TAsset>(name);
        }

        internal static TAsset? SearchInner<TAsset>(string name) where TAsset : Asset
        {
            return FileSystem.SearchInner<TAsset>(name);
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
            return SearchInner<TAsset>(parent, name);
        }

        internal static TAsset? SearchInner<TAsset>(VirtualFileSystem.Directory parent, string name) where TAsset : Asset
        {
            return FileSystem.SearchInner<TAsset>(parent, name);
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
            return SearchInner(name);
        }

        internal static Asset? SearchInner(string name)
        {
            return (Asset?)FileSystem.SearchInner(name);
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
            return SearchInner(parent, name);
        }

        internal static Asset? SearchInner(VirtualFileSystem.Directory parent, string name)
        {
            return (Asset?)FileSystem.SearchInner(parent, name);
        }

        #endregion

        #region Import from File

        /// <summary>
        /// Import a new asset into the virtual file system and create it if import fails
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="targetPath"></param>
        /// <param name="name"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [ExposedApi]
        public static Asset? Import(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportInner(assetType, createOnFail, targetPath, name, perm, sourcePath, args);
        }

        internal static Asset? ImportInner(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, createOnFail, false, null, targetPath, name, perm, sourcePath, args);
        }

        /// <summary>
        /// Import a new asset into the virtual file system and create it if import fails
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="assetType"></param>
        /// <param name="targetPath"></param>
        /// <param name="name"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TAsset? Import<TAsset>(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportInner<TAsset>(assetType, createOnFail, targetPath, name, perm, sourcePath, args);
        }

        internal static TAsset? ImportInner<TAsset>(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)ImportInner(assetType, createOnFail, targetPath, name, perm, sourcePath, args);
        }

        #endregion

        #region Import from Stream

        /// <summary>
        /// Import a new asset into the virtual file system
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        /// <param name="stream"></param>
        /// <param name="targetPath"></param>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static Asset? Import(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportInner(assetType, stream, targetPath, name, perm, sourcePath, args);
        }

        internal static Asset? ImportInner(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            return InnerInstance.Import(assetType, false, false, stream, targetPath, name, perm, sourcePath, args);
        }

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
        /// <returns></returns>
        [ExposedApi]
        public static TAsset? Import<TAsset>(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportInner<TAsset>(assetType, stream, targetPath, name, perm, sourcePath, args);
        }

        internal static TAsset? ImportInner<TAsset>(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args) where TAsset : Asset
        {
            return (TAsset?)ImportInner(assetType, stream, targetPath, name, perm, sourcePath, args);
        }

        #region Lazy Imports

        [ExposedApi]
        public static LazyAsset? ImportLazy(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportLazyInner(assetType, createOnFail, targetPath, name, perm, sourcePath, args);
        }

        internal static LazyAsset? ImportLazyInner(string assetType, bool createOnFail, string targetPath, string name, Permissions perm, string sourcePath, params dynamic[] args)
        {
            return (LazyAsset?)InnerInstance.Import(assetType, createOnFail, true, null, targetPath, name, perm, sourcePath, args);
        }

        [ExposedApi]
        public static LazyAsset? ImportLazy(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return ImportLazyInner(assetType, stream, targetPath, name, perm, sourcePath, args);
        }

        internal static LazyAsset? ImportLazyInner(string assetType, Stream stream, string targetPath, string name,
            Permissions perm, string sourcePath, params dynamic[] args)
        {
            return (LazyAsset?)InnerInstance.Import(assetType, false, true, stream, targetPath, name, perm, sourcePath, args);
        }

        #endregion

        #endregion

        public static string? GetTypeFromFileExtension(string fileName)
        {
            return InnerInstance.GetTypeFromFileExtension(fileName);
        }

        /// <summary>
        /// Implementation of the public API of the AssetManager
        /// </summary>
        private class Inner
        {
            private Inner()
            {
                AssetHandlers = new Dictionary<string, Dictionary<string, HandlerFunc>>();
                FileExtensionAssetTypes = new Dictionary<string, string>();

                RegisterAssetType("text/plain", new[]{".txt"},
                    (name, perms, sourcePath, args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/plain asset: wrong number of arguments");
                        return new TextAsset(name, perms, sourcePath);
                    });

                RegisterAssetType("text/xml", new[] { ".xml" },
                    (name, perm, sourcePath, args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/xml asset: wrong number of arguments");
                        return new XmlAsset(name, perm, sourcePath);
                    });

                RegisterAssetType("text/json", new[] { ".json" },
                    (name, perm, sourcePath, args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/json asset: wrong number of arguments");
                        return new JsonAsset(name, perm, sourcePath);
                    });

                RegisterAssetType("text/css", new[] { ".css" },
                   (name, perm, sourcePath, args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of text/css asset: wrong number of arguments");
                       return new CssAsset(name, perm, sourcePath);
                   });

                RegisterAssetType("image/sprite", new[] { ".png", ".jpeg" },
                   (name, perm, sourcePath, args) =>
                   {
                       if (args.Length != 0)
                           throw new Exception("Import of image/sprite asset: wrong number of arguments");
                       return new SpriteAsset(name, perm, sourcePath);
                   });
                
                RegisterAssetType("text/uxml", new[] { ".uxml" },
                    (name, perm, sourcePath, args) =>
                    {
                        if (args.Length != 0)
                            throw new Exception("Import of text/uxml asset: wrong number of arguments");
                        return new VisualElementAsset(name, perm, sourcePath);
                    });

                RegisterAssetType("text/gltf", new[] { ".gltf", ".glb" },
                     (name, perm, sourcePath, args) =>
                     {
                       if (args.Length != 0)
                           throw new Exception("Import of text/gltf asset: wrong number of arguments");
                       return new GltfAsset(name, perm, sourcePath);
                   });
            }

            private Dictionary<string, Dictionary<string, HandlerFunc>> AssetHandlers { get; }
            private Dictionary<string, string> FileExtensionAssetTypes { get; }

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

            public void RegisterAssetType(string assetType, string[] fileExtensions, HandlerFunc handler)
            {
                string[] types = SplitAssetType(assetType);
                RegisterAsset(types[0], types[1], fileExtensions, handler);
            }

            public void RegisterAsset(string type, string subtype, string[] fileExtensions, HandlerFunc handler)
            {
                if(AssetHandlers.ContainsKey(type) && AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception($"Registering duplicate handler for {type}/{subtype}");
                }

                if (!AssetHandlers.ContainsKey(type))
                {
                    AssetHandlers[type] = new Dictionary<string, HandlerFunc>();
                }

                foreach (var fileExtension in fileExtensions)
                {
                    if (FileExtensionAssetTypes.ContainsKey(fileExtension))
                    {
                        throw new Exception($"Registering duplicate handler for asset type with file extension {fileExtension}");
                    }
                    FileExtensionAssetTypes[fileExtension] = type + "/" + subtype;
                }

                AssetHandlers[type][subtype] = handler;
            }

            public Asset? GetAsset(string targetPath) => (Asset?)FileSystem.TraverseInner(targetPath);

            public string? GetTypeFromFileExtension(string fileName)
            {
                return FileExtensionAssetTypes.TryGetValue(Path.GetExtension(fileName), out string type) ? type : null;
            }

            public Asset? Import(string assetType, bool createOnFail, bool lazyImport, Stream? sourceStream, string targetPath, string name,
                Permissions perm, string sourcePath, params dynamic[] args)
            {
                var types = SplitAssetType(assetType);
                return Import(types[0], types[1], createOnFail, lazyImport, sourceStream, targetPath, name, perm, sourcePath, args);
            }

            public Asset? Import(string type, string subtype, bool createOnFail, bool lazyImport, Stream? sourceStream, string targetPath,
                string name, Permissions perm, string sourcePath, params dynamic[] args)
            {
                if (!AssetHandlers.ContainsKey(type) || !AssetHandlers[type].ContainsKey(subtype))
                {
                    throw new Exception($"Importing asset with unregistered type {type}/{subtype}");
                }

                string path = FileSystem.BasePath + sourcePath;

                if (sourceStream == null && sourcePath != "") // We need to read it
                {
                    if (!File.Exists(path))
                    {
                        if (createOnFail)
                        {
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
                            sourceStream = File.Create(path);
                        }
                    }
                    else
                    {
                        sourceStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                    }
                }

                if (sourceStream == null)
                {
                    throw new Exception("Source stream for import is empty");
                }

                Asset? newAsset = AssetHandlers[type][subtype](name, perm, path, args);

                if (newAsset == null)
                {
                    return null;
                }

                EventBus.EventBus.Push(AssetImportEvent.Tag, new AssetImportEvent(name, targetPath, type + "/" + subtype));

                if (lazyImport)
                {
                    LazyAsset lazyAsset = new LazyAsset(newAsset, sourceStream, targetPath);
                    return (Asset?)FileSystem.AddEntry(targetPath, lazyAsset.Load(new byte[0]));
                }

                int streamLength = (int)sourceStream.Length;

                byte[] data = new byte[streamLength];
                sourceStream.Read(data, 0, streamLength);

                sourceStream.Close();

                // TODO make it so we don't have to allocate twice the size of the asset every
                // time we import it (i.e. a 500 KB asset will result in 1000 KB of allocation)
                return (Asset?)FileSystem.AddEntry(targetPath, newAsset.Load(data));
            }

            public static readonly Inner InnerInstance = new Inner();
        }
        private static Inner InnerInstance => Inner.InnerInstance;
    }
}