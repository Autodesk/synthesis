using System;
using System.IO;
using System.Reflection;

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
        public static string BasePath = string.Format("{0}{1}Autodesk{1}Synthesis{1}",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Path.DirectorySeparatorChar);

        public static string TestPath = BasePath + $"test{Path.DirectorySeparatorChar}";

        /// <summary>
        /// Add a new resource to a the file system at a given destination
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static TResource? AddResource<TResource>(string path, TResource resource) where TResource : class, IEntry
        {
            Directory? parentDir = Traverse<Directory>(path);

            return parentDir?.AddResource<TResource>(resource);
        }

        /// <summary>
        /// Add a new resource to the file system at a given desitnation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IEntry? AddResource(string path, IEntry resource)
        {
            Directory? parentDir = Traverse<Directory>(path);

            return parentDir?.AddResource(resource);
        }

        public static void RemoveResource(string path, string name, Guid guid)
        {
            Directory? parentDir = (Directory?)Traverse(path);

            parentDir?.RemoveEntry(name, guid);
        }

        public static bool ResourceExists(string path, string name)
        {
            Directory? parentDir = (Directory?)Traverse(path);

            return parentDir != null && parentDir.EntryExists(name);
        }

		/// <summary>
        /// Determine the depth of a given path (i.e. the number of nested directories)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int DepthOfPath(string path)
        {
            return path.Split('/').Length;
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TResource? Traverse<TResource>(string path) where TResource : class, IEntry
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.Traverse<TResource>(path);
        }


        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEntry? Traverse(string[] path)
        {
            if (path.Length >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.Traverse(path);
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEntry? Traverse(string path)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception($"FileSystem: traversing path would exceed maximum directory depth of {MaxDirectoryDepth}");
            }
            return Instance.RootNode.Traverse(path);
        }

        /// <summary>
        /// Recursively search the virtual file system for an entry with a given name
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TEntry? Search<TEntry>(string name) where TEntry : class, IEntry
        {
            return Search<TEntry>(Instance.RootNode, name);
        }

        /// <summary>
        /// Recursively search a directory for an entry with a given name
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TEntry? Search<TEntry>(Directory parent, string name) where TEntry : class, IEntry
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
                    var result = Search<TEntry>(directory, name);
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
        public static IEntry? Search(string name)
        {
            return Search(Instance.RootNode, name);
        }

        /// <summary>
        /// Recursively search a directory for an entry with a given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEntry? Search(Directory parent, string name)
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
                    var result = Search(directory, name);
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
            public Inner()
            {
                RootNode = new Directory(""); // root node name is "" so paths begin with "/" (since path strings are split at '/')
                RootNode.AddResource(new Directory("environment"));
                RootNode.AddResource(new Directory("modules"));
                RootNode.AddResource(new Directory("temp"));
            }

            /// <summary>
            /// The root of the file system
            /// </summary>
            public Directory RootNode { get; private set; }

            internal static readonly Inner instance = new Inner();
        }

        private static Inner Instance { get { return Inner.instance; } }
    }
}
