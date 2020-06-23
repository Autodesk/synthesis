using System;
using System.IO;
using System.Reflection;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A virtual file system that manages resources and assets as a tree
    /// </summary>
    public static class FileSystem // TODO static or singleton pattern?
    {
        /// <summary>
        /// Maximum number of nested directories allowed
        /// </summary>
        public const int MaxDirectoryDepth = 50; // TODO pick maximum directory depth

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
        public static TResource AddResource<TResource>(string path, TResource resource) where TResource : IResource
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = (Directory)Traverse(path);

            return parent_dir.AddEntry<TResource>(resource);
        }

        /// <summary>
        /// Add a new resource to the file system at a given desitnation
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IResource AddResource(string path, IResource resource)
        {
            if (DepthOfPath(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = (Directory)Traverse(path);

            return parent_dir.AddEntry(resource);
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
        /// Initialize the file system. This must be done before it can be used
        /// </summary>
        public static void Init()
        {
            RootNode = new Directory(""); // root node name is "" so paths begin with "/" (since path strings are split at '/')
            RootNode.AddEntry(new Directory("environment"));
            RootNode.AddEntry(new Directory("modules"));
            RootNode.AddEntry(new Directory("temp"));
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResource Traverse(string[] path)
        {
            return RootNode.Traverse(path);
        }

        /// <summary>
        /// Traverse the file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IResource Traverse(string path)
        {
            return RootNode.Traverse(path);
        }

        /// <summary>
        /// The root of the file system
        /// </summary>
        public static Directory RootNode { get; private set; }
    }
}
