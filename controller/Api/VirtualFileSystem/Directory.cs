using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Api.VirtualFileSystem
{
    public class Directory : IResource
    {

        public Directory()
        {
            Owner = Guid.Empty;
            Permissions = Permissions.Private;
            Entries = new Dictionary<string, IResource>();
        }

        public Directory(Guid owner, Permissions perm)
        {
            Owner = owner;
            Permissions = perm;
        }

        public string Name { get; }

        public Guid Owner { get; }

        public Permissions Permissions { get; }

        internal Dictionary<string, IResource> Entries;

        public IResource Traverse(string[] subpaths)
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

        public IResource Traverse(string path)
        {
            return Traverse(path.Split('/'));
        }

        public IResource TryGetEntry(string key)
        {
            return Entries.TryGetValue(key, out var x) ? x : null;
        }

        public IResource GetEntry(string key)
        {
            if (!Entries.ContainsKey(key))
            {
                throw new Exception();
            }
            return Entries[key];
        }

        public void AddEntry(string key, IResource value)
        {
            Entries.Add(key, value);
        }

        public void AddEntry<TResource>(string name) where TResource : IResource, new()
        {
            Entries.Add(name, new TResource());
        }

        public IResource this[string name]
        {
            get => TryGetEntry(name);
            set => Entries[name] = value;
        }

        public void Delete()
        {
            // TODO
        }
    }
}
