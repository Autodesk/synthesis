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
            Entries = new Dictionary<string, Resource>();
            Entries.Add("", this);
            Entries.Add(".", this);
            Parent = null;
            Entries.Add("..", Parent);
        }

        internal Dictionary<string, Resource> Entries;
        
        public Resource Traverse(string[] subpaths)
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

        public Resource Traverse(string path)
        {
            return Traverse(path.Split('/')); // TODO should we trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
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

        public Resource AddEntry(Resource value)
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

        public Resource this[string name]
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
