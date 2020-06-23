using System;
using System.IO;
using System.Reflection;

namespace SynthesisAPI.VirtualFileSystem
{
    public static class FileSystem // TODO static or singleton pattern?
    {
        public const int MaxDirectoryDepth = 50; // TODO pick maximum directory depth

        public static string BasePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).ToString()).ToString() + Path.DirectorySeparatorChar;
        // public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar;

        private static Directory GetDirectory(string path)
        {
            IResource parent_dir = RootNode.Traverse(path);

            if (parent_dir == null)
            {
                // TODO
                throw new Exception();
            }

            if (parent_dir.GetType() != typeof(Directory))
            {
                // TODO
                throw new Exception();
            }

            return (Directory)parent_dir;
        }

        public static TResource AddResource<TResource>(string path, TResource resource) where TResource : IResource
        {
            if (PathToDepth(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = GetDirectory(path);

            return parent_dir.AddEntry<TResource>(resource);
        }


        public static IResource AddResource(string path, IResource resource)
        {
            if (PathToDepth(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = GetDirectory(path);

            return parent_dir.AddEntry(resource);
        }

        public static void RemoveResource(string path, string name, Guid guid)
        {
            if (PathToDepth(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = GetDirectory(path);

            parent_dir.RemoveEntry(name, guid);
        }

        public static bool ResourceExists(string path, string name)
        {
            if (PathToDepth(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = GetDirectory(path);

            return parent_dir.EntryExists(name);
        }

        public static int PathToDepth(string path)
        {
            return path.Split('/').Length;
        }

        public static void Init()
        {
            RootNode = new Directory(""); // root node name is "" so paths begin with "/" (since path strings are split at '/')
            RootNode.AddEntry(new Directory("environment"));
            RootNode.AddEntry(new Directory("modules"));
            RootNode.AddEntry(new Directory("temp"));
        }

        public static IResource Traverse(string[] path)
        {
            return RootNode.Traverse(path);
        }

        public static IResource Traverse(string path)
        {
            return RootNode.Traverse(path);
        }

        public static Directory RootNode { get; private set; }
    }
}
