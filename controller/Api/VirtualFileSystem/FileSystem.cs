using System;
using System.Collections.Generic;

namespace Api.VirtualFileSystem
{
    public static class FileSystem
    {
        public static void AddResource<TResource>(string path, string name, Guid owner, Permissions perm) where TResource : IResource, new()
        {
            var dir = rootNode.Traverse(path);
            if (dir == null)
            {
                // TODO
                throw new Exception();
            }
            if (dir.GetType() != typeof(Directory))
            {
                // TODO
                throw new Exception();
            }

            ((Directory) dir).AddEntry(name, new TResource()); // TODO

        }

        public static void Init()
        {
            rootNode = new Directory();
            rootNode.AddEntry<Directory>("world");
            rootNode.AddEntry<Directory>("modules");
        }

        private static Directory rootNode;
    }
}
