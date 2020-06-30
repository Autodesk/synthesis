using SynthesisAPI.Utilities;
using System;
using System.IO;

#nullable enable

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A virtual file system that manages resources and assets as a tree
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// Maximum number of nested directories allowed
        /// </summary>
        public const int MaxDirectoryDepth = 30;

        /// <summary>
        /// Base path for files on disk
        /// </summary>
        public static readonly string BasePath = string.Format("{0}{1}Autodesk{1}Synthesis{1}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Path.DirectorySeparatorChar);

        public static readonly string TestPath = BasePath + $"test{Path.DirectorySeparatorChar}";

        /// <summary>
        /// Add a new resource to a the file system at a given destination
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TResource? AddResource<TResource>(string path, TResource resource, Permissions perm = Permissions.PublicReadWrite) where TResource : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();
            return AddResourceInner(path, resource, perm);
        }

        private static TResource? AddResourceInner<TResource>(string path, TResource resource, Permissions perm = Permissions.PublicReadWrite) where TResource : class, IEntry
        {
            Directory? parentDir = CreatePathInner(path, perm);

            return parentDir?.AddResourceInner(resource);
        }

        /// <summary>
        /// Add a new resource to the file system at a given desitnation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? AddResource(string path, IEntry resource, Permissions perm = Permissions.PublicReadWrite)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddResourceInner(path, resource, perm);
        }

        internal static IEntry? AddResourceInner(string path, IEntry resource, Permissions perm = Permissions.PublicReadWrite)
        {
            Directory? parentDir = CreatePathInner(path, perm);

            return parentDir?.AddResource(resource);
        }

        [ExposedApi]
        public static void RemoveResource(string path, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            RemoveResourceInner(path, name);
        }

        internal static void RemoveResourceInner(string path, string name)
        {
            Directory? parentDir = (Directory?)TraverseInner(path);

            parentDir?.RemoveEntryInner(name);
        }

        [ExposedApi]
        public static bool ResourceExists(string path, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return ResourceExistsInner(path, name);
        }

        internal static bool ResourceExistsInner(string path, string name)
        {
            Directory? parentDir = (Directory?)TraverseInner(path);

            return parentDir != null && parentDir.EntryExistsInner(name);
        }

        /// <summary>
        /// Determine the depth of a given path (i.e. the number of nested directories)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int DepthOfPath(string path)
        {
            return Directory.SplitPath(path).Length;
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TResource? Traverse<TResource>(string path) where TResource : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner<TResource>(path);
        }

        internal static TResource? TraverseInner<TResource>(string path) where TResource : class, IEntry
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.TraverseInner<TResource>(path);
        }

        [ExposedApi]
        public static Directory? CreatePath(string path, Permissions perm)
        {
            return CreatePathInner(path, perm);
        }

        internal static Directory? CreatePathInner(string path, Permissions perm)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            string[] subpaths = Directory.SplitPath(path);
            string top;
            
            (top, subpaths) = Directory.GetTopDirectory(subpaths);

            Directory? dir = Instance.RootNode.TraverseInner<Directory>(top);
            
            while(subpaths.Length > 0) {
                (top, subpaths) = Directory.GetTopDirectory(subpaths);

                if (!dir.EntryExistsInner<Directory>(top))
                {
                    if (dir.EntryExistsInner(top))
                    {
                        throw new Exception("Attempting to create directory with same name as another entry \"" + top + "\"");
                    }
                    dir = dir.AddResourceInner(new Directory(top, perm));
                }
                else
                {
                    dir = dir.TraverseInner<Directory>(dir.Name + "/" + top);
                }
                if(dir == null)
                {
                    throw new Exception("Failed to create directory");
                }
            }
            return dir;
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? Traverse(string[] path)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner(path);
        }

        internal static IEntry? TraverseInner(string[] path)
        {
            if (path.Length >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.TraverseInner(path);
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? Traverse(string path)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner(path);
        }

        internal static IEntry? TraverseInner(string path)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.TraverseInner(path);
        }

        /// <summary>
        /// Recursively search the virtual file system for an entry with a given name
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TEntry? Search<TEntry>(string name) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return SearchInner<TEntry>(Instance.RootNode, name);
        }

        internal static TEntry? SearchInner<TEntry>(string name) where TEntry : class, IEntry
        {
            return SearchInner<TEntry>(Instance.RootNode, name);
        }

        /// <summary>
        /// Recursively search a directory for an entry with a given name
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TEntry? Search<TEntry>(Directory parent, string name) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return SearchInner<TEntry>(parent, name);
        }

        internal static TEntry? SearchInner<TEntry>(Directory parent, string name) where TEntry : class, IEntry
        {
            var entry = parent[name];
            if (entry != null && entry is TEntry)
            {
                return (TEntry?)entry;
            }
            foreach (var e in parent.Entries)
            {
                if (e.Value is Directory directory)
                {
                    var result = SearchInner<TEntry>(directory, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively search the virtual file system for an entry with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? Search(string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return SearchInner(Instance.RootNode, name);
        }

        internal static IEntry? SearchInner(string name)
        {
            return SearchInner(Instance.RootNode, name);
        }

        /// <summary>
        /// Recursively search a directory for an entry with a given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? Search(Directory parent, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return SearchInner(parent, name);
        }

        internal static IEntry? SearchInner(Directory parent, string name)
        {
            var asset = parent[name];
            if (asset != null)
            {
                return asset;
            }
            foreach (var e in parent.Entries)
            {
                if (e.Value is Directory directory)
                {
                    var result = SearchInner(directory, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private class Inner
        {
            private Inner()
            {
                using var _ = ApiCallSource.ForceInternalCall();
                RootNode = new Directory("", Permissions.PublicReadOnly); // root node name is "" so paths begin with "/" (since path strings are split at '/')
                RootNode.AddResourceInner(new Directory("environment", Permissions.PublicReadOnly));
                RootNode.AddResourceInner(new Directory("modules", Permissions.PublicReadOnly));
                RootNode.AddResourceInner(new Directory("temp", Permissions.PublicReadWrite));
            }

            /// <summary>
            /// The root of the file system
            /// </summary>
            public Directory RootNode { get; private set; }

            public static readonly Inner Instance = new Inner();
        }

        private static Inner Instance => Inner.Instance;
    }
}
