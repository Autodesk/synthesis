using System;
using System.Collections.Generic;

namespace SynthesisAPI.VirtualFileSystem
{
    public static class FileSystem
    {
        
        public static void AddResource<TResource>(string path, string name, Guid owner, Permissions perm) where TResource : Resource, new()
        {
            Resource x = rootNode.Traverse(path);

            if (x == null)
            {
                // TODO
                throw new Exception();
            }

            if (x.GetType() != typeof(Directory))
            {
                // TODO
                throw new Exception();
            }

            Directory dir = (Directory)x;

            Resource entry = dir.AddEntry<TResource>(name);
            
            if (typeof(TResource) == typeof(Directory))
            {
                ((Directory)entry).Init(name, owner, perm, dir.Depth + 1);
            }
            else
            {
                entry.Init(name, owner, perm);
            }
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
