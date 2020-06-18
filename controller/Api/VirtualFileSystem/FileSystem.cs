using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.VirtualFileSystem
{
    public static class FileSystem
    {
        public const int MaxDirectoryDepth = 50; // TODO pick maximum directory depth

        private static Directory GetDirectory(string path)
        {
            Resource parent_dir = RootNode.Traverse(path);

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

        public static Resource AddResource(string path, Resource resource)
        {
            if (PathToDepth(path) >= MaxDirectoryDepth)
            {
                throw new Exception();
            }

            Directory parent_dir = GetDirectory(path);

            return parent_dir.AddEntry(resource);
        }

        public static int PathToDepth(string path)
        {
            return path.Split('/').Length;
        }

        public static void Init()
        {
            RootNode = new Directory(""); // root node name is "" so paths begin with "/" (since path strings are split at '/')
            RootNode.AddEntry(new Directory("world"));
            RootNode.AddEntry(new Directory("modules"));
        }

        public static Resource Traverse(string[] path)
        {
            return RootNode.Traverse(path);
        }

        public static Resource Traverse(string path)
        {
            return RootNode.Traverse(path);
        }

        public static Directory RootNode { get; private set; }
    }
}
