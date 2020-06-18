using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.VirtualFileSystem
{
    public static class FileSystem
    {
        private static Directory GetDirectory(string path)
        {
            Resource parent_dir = rootNode.Traverse(path);

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
            if (PathToDepth(path) >= 50) // TODO maximum directory depth
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
            rootNode = new Directory(""); // root node name is "" so paths begin with "/" (since path strings are split at '/')
            rootNode.AddEntry(new Directory("world"));
            rootNode.AddEntry(new Directory("modules"));
        }

        private static Directory rootNode;
    }
}
