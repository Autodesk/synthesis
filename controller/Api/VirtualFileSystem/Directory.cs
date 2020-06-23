using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SynthesisAPI.VirtualFileSystem
{
    public class Directory : Resource
    {
        public Directory(string name) : this(name, Guid.Empty, Permissions.Private) { }

        public Directory(string name, Guid owner, Permissions perm)
        {
            Init(name, owner, perm);
            Entries = new Dictionary<string, IResource>();
            Entries.Add("", this);
            Entries.Add(".", this);
            SetParent(null);
            Entries.Add("..", Parent);
        }

        internal Dictionary<string, IResource> Entries;
        
        public IResource Traverse(string[] subpaths)
        {
            if (subpaths.Length == 0)
            {
                return null;
            }

            string target = subpaths[0];
            subpaths = subpaths.Skip(1).ToArray();

            if (target != Name)
            {
                if (target == ".." && Parent != null)
                {
                    if (subpaths.Length == 0)
                    {
                        return Parent;
                    }
                    return Parent.Traverse(subpaths);
                }
                return null;
            }

            if(subpaths.Length == 0)
            {
                return this;
            }

            var next = this[subpaths[0]];

            if (subpaths.Length == 1)
            {
                return next;
            }

            if (next == null)
            {
                return null;
            }

            if (next.GetType() == typeof(Directory))
            {
                return ((Directory) next).Traverse(subpaths);
            }

            return null;
        }

        public IResource Traverse(string path)
        {
            return Traverse(path.Split('/')); // TODO should we trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
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

        public TResource AddEntry<TResource>(TResource value) where TResource : IResource
        {
            return (TResource)AddEntryImpl(value);
        }

        private IResource AddEntry(IResource value)
        {
            return AddEntryImpl(value);
        }

        private IResource AddEntryImpl(IResource value)
        {
            if (Entries.ContainsKey(value.Name))
            {
                throw new Exception();
            }
            Entries.Add(value.Name, value);

            // Set this as the parent of the resource, and add the parent to its entries
            // if the resource is a directory
            value.Parent = this;
            if (value.GetType() == typeof(Directory))
            {
                Directory dir = (Directory)value;
                if (dir.Entries[".."] != null)
                {
                    throw new Exception();
                }
                dir.Entries[".."] = dir.Parent;
            }

            return Entries[value.Name];
        }

        public IResource this[string name]
        {
            get => TryGetEntry(name);
            set => Entries[name] = value;
        }

        public override void Delete()
        {
            // TODO
        }
    }
}
