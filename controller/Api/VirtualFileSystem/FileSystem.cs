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
        public static string BasePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).ToString()).ToString() + Path.DirectorySeparatorChar;
        // public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Add a new resource to a the file system at a given destination
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static TResource? AddResource<TResource>(string path, TResource resource) where TResource : class, IResource
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory? parent_dir = Traverse<Directory>(path);

            return parent_dir?.AddResource<TResource>(resource);
        }

        /// <summary>
        /// Add a new resource to the file system at a given desitnation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IResource? AddResource(string path, IResource resource)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory? parent_dir = Traverse<Directory>(path);

            return parent_dir?.AddResource(resource);
        }

        public static void RemoveResource(string path, string name, Guid guid)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = (Directory)Traverse(path);

            parent_dir.RemoveEntry(name, guid);
        }

        public static bool ResourceExists(string path, string name)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = (Directory)Traverse(path);

            return parent_dir.EntryExists(name);
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
        public static TResource? Traverse<TResource>(string path) where TResource : class, IResource
        {
            return Instance.RootNode.Traverse<TResource>(path);
        }


        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResource? Traverse(string[] path)
        {
            return Instance.RootNode.Traverse(path);
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResource? Traverse(string path)
        {
            return Instance.RootNode.Traverse(path);
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
