using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SynthesisAPI.VirtualFileSystem
{
    public class Directory : Resource
    {

        public Directory()
        {
            Owner = Guid.Empty;
            Permissions = Permissions.Private;
            Entries = new Dictionary<string, Resource>();
            Depth = 0;
        }

        public Directory(Guid owner, Permissions perm, uint depth)
        {
            Owner = owner;
            Permissions = perm;
            Depth = depth;
        }

        internal Dictionary<string, Resource> Entries;

        public uint Depth { get; internal set; }

        public Resource Traverse(string[] subpaths)
        {
            if (subpaths.Length == 0 || subpaths[0] != Name)
            {
                return null;
            }

            var next = this[subpaths[0]];

            if (next == null)
            {
                return null;
            }

            subpaths = subpaths.Skip(1).ToArray();
            
            if(subpaths.Length == 0)
            {
                return next;
            }

            if (next.GetType() == typeof(Directory))
            {
                return ((Directory) next).Traverse(subpaths);
            }

            return null;
        }

        public Resource Traverse(string path)
        {
            return Traverse(path.Split('/'));
        }

        public Resource TryGetEntry(string key)
        {
            return Entries.TryGetValue(key, out var x) ? x : null;
        }

        public Resource GetEntry(string key)
        {
            if (!Entries.ContainsKey(key))
            {
                throw new Exception();
            }
            return Entries[key];
        }

        public Resource AddEntry(string key, Resource value)
        {
            Entries.Add(key, value);
            return Entries[key];
        }

        public Resource AddEntry<TResource>(string key) where TResource : Resource, new()
        {
            Entries.Add(key, new TResource());
            return Entries[key];
        }

        public Resource this[string name]
        {
            get => TryGetEntry(name);
            set => Entries[name] = value;
        }

        public void Init(string name, Guid owner, Permissions perm, uint depth)
        {
            Init(name, owner, perm);
            Depth = depth;
        }

        public void Delete()
        {
            // TODO
        }
    }
}
