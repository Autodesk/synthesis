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
        }

        internal Dictionary<string, Resource> Entries;

        // TODO add parent directory
        
        public Resource Traverse(string[] subpaths)
        {
            if (subpaths.Length == 0 || subpaths[0] != Name)
            {
                return null;
            }

            subpaths = subpaths.Skip(1).ToArray();

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
