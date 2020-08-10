using SynthesisAPI.Utilities;
using System;
using System.IO;

#nullable enable

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A virtual file system that manages entries and assets as a tree
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

        public static readonly string TestPathLocal = $"test{Path.DirectorySeparatorChar}";

        public static readonly string TestPath = BasePath + TestPathLocal;

        /// <summary>
        /// Add a new entry to a the file system at a given destination
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="path"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TEntry? AddEntry<TEntry>(string path, TEntry entry, Permissions perm = Permissions.PublicReadWrite) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();
            return AddEntryInner(path, entry, perm);
        }

        private static TEntry? AddEntryInner<TEntry>(string path, TEntry entry, Permissions perm = Permissions.PublicReadWrite) where TEntry : class, IEntry
        {
            CheckPath(Directory.MakePath(path, entry.Name));
            Directory? parentDir = CreatePathInner(path, perm);

            return parentDir?.AddEntryInner(entry);
        }

        /// <summary>
        /// Add a new entry to the file system at a given desitnation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        [ExposedApi]
        public static IEntry? AddEntry(string path, IEntry entry, Permissions perm = Permissions.PublicReadWrite)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddEntryInner(path, entry, perm);
        }

        internal static IEntry? AddEntryInner(string path, IEntry entry, Permissions perm = Permissions.PublicReadWrite)
        {
            CheckPath(Directory.MakePath(path, entry.Name));

            Directory? parentDir = CreatePathInner(path, perm);

            return parentDir?.AddEntry(entry);
        }

        [ExposedApi]
        public static void DeleteEntry(string path)
        {
            using var _ = ApiCallSource.StartExternalCall();

            DeleteEntryInner(path);
        }

        internal static void DeleteEntryInner(string path)
        {
            TraverseInner(path)?.DeleteInner();
        }

        [ExposedApi]
        public static void DeleteEntry(string path, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            DeleteEntryInner(path, name);
        }

        internal static void DeleteEntryInner(string path, string name)
        {
            Directory? parentDir = (Directory?)TraverseInner(path);

            parentDir?.DeleteEntryInner(name);
        }

        [ExposedApi]
        public static bool EntryExists(string path)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return EntryExistsInner(path);
        }

        internal static bool EntryExistsInner(string path)
        {
            return TraverseInner(path) != null;
        }

        [ExposedApi]
        public static bool EntryExists(string path, string name)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return EntryExistsInner(path, name);
        }

        internal static bool EntryExistsInner(string path, string name)
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

        private static string[] CheckPath(string path)
        {
            return CheckPath(Directory.SplitPath(path));
        }

        private static string[] CheckPath(string[] path)
        {
            var (top, actualPath) = Directory.GetTopDirectory(path);
            if (top != Instance.RootNode.Name)
            {
                throw new DirectoryException($"Path outside of virtual file system \"{string.Join(Directory.DirectorySeparatorChar.ToString(), path)}\"");
            }
            if (path.Length > MaxDirectoryDepth)
            {
                throw new DirectroyDepthExpection(path.Length);
            }
            return actualPath;
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ExposedApi]
        public static TEntry? Traverse<TEntry>(string path) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner<TEntry>(path);
        }

        internal static TEntry? TraverseInner<TEntry>(string path) where TEntry : class, IEntry
        {
            var actualPath = CheckPath(path);
            return Instance.RootNode.TraverseInner<TEntry>(actualPath);
        }

        [ExposedApi]
        public static Directory? CreatePath(string path, Permissions perm)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return CreatePathInner(path, perm);
        }

        internal static Directory? CreatePathInner(string path, Permissions perm)
        {
            var subpaths = CheckPath(path);
            string top;

            Directory? dir = Instance.RootNode;
            
            while(subpaths.Length > 0) {
                (top, subpaths) = Directory.GetTopDirectory(subpaths);

                if (!dir.EntryExistsInner<Directory>(top))
                {
                    dir = dir.AddEntryInner(new Directory(top, perm));
                }
                else
                {
                    dir = dir.TraverseInner<Directory>(top);
                }
                if(dir == null)
                {
                    throw new DirectoryException("Failed to create directory");
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
            var actualPath = CheckPath(path);
            return Instance.RootNode.TraverseInner(actualPath);
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
            var actualPath = CheckPath(path);
            return Instance.RootNode.TraverseInner(actualPath);
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
                if (e.Key == "." || e.Key == "..")
                    continue;
                
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
                if (e.Key == "." || e.Key == "..")
                    continue;
                
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
                RootNode.AddEntryInner(new Directory("environment", Permissions.PublicReadWrite));
                RootNode.AddEntryInner(new Directory("modules", Permissions.PublicReadWrite));
                RootNode.AddEntryInner(new Directory("temp", Permissions.PublicReadWrite));
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
