using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

#nullable enable

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A wrapper for a dictionary of resources that gives structure to the virtual file system
    /// </summary>
    public class Directory : IEntry
    {
        /// <summary>
        /// Initialize Resource data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        private void Init(string name, Guid owner, Permissions perm)
        {
            _name = name;
            _owner = owner;
            _permissions = perm;
            _parent = null;
        }

        public string Name => ((IEntry)this).Name;
        public Guid Owner => ((IEntry)this).Owner;
        public Permissions Permissions => ((IEntry)this).Permissions;
        private Directory Parent => ((IEntry)this).Parent;

        private string _name { get; set; }
        private Guid _owner { get; set; }
        private Permissions _permissions { get; set; }
        private Directory _parent { get; set; }

        string IEntry.Name { get => _name; set => _name = value; }
        Guid IEntry.Owner { get => _owner; set => _owner = value; }
        Permissions IEntry.Permissions { get => _permissions; set => _permissions = value; }
        Directory IEntry.Parent { get => _parent; set => _parent = value; }

        internal Dictionary<string, IEntry> Entries;

        public Directory(string name, Guid owner, Permissions perm)
        {
            Init(name, owner, perm);
            Entries = new Dictionary<string, IEntry>();
            Entries.Add("", this);
            Entries.Add(".", this);
            Entries.Add("..", null!);
        }

        [ExposedApi]
        public virtual void Delete()
        {
            using var _ = ApiCallSource.StartExternalCall();

            DeleteInner();
        }

        internal virtual void DeleteInner() {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            foreach (var e in Entries)
            {
                if (e.Key != "" && e.Key != "." && e.Key != "..")
                {
                    e.Value.DeleteInner();
                }
            }
        }

        void IEntry.DeleteInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
        }

        private IEntry? TraverseImpl(string[] subpaths)
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
                    return Parent.TraverseInner(subpaths);
                }
                return null;
            }

            if(subpaths.Length == 0)
            {
                return this;
            }

            var next = TryGetEntryInner(subpaths[0]);

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
                return ((Directory)next).TraverseImpl(subpaths);
            }

            return null;
        }

        /// <summary>
        /// Traverse this Directory and subdirectories to a resource given a file path
        /// </summary>
        /// <param name="subpaths"></param>
        /// <returns></returns>
        [ExposedApi]
        public IEntry? Traverse(string[] subpaths)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner(subpaths);
        }


        internal IEntry? TraverseInner(string[] subpaths)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseImpl(subpaths);
        }

        /// <summary>
        /// Traverse this Directory and subdirectories to a resource given a file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ExposedApi]
        public IEntry? Traverse(string path)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner(path);
        }

        internal IEntry? TraverseInner(string path)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            if (path.Length > 0 && path[path.Length - 1] == '/')
            {
                // trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
                path = path.Remove(path.Length - 1, 1);
            }
            return TraverseImpl(path.Split('/'));
        }

        [ExposedApi]
        public TResource? Traverse<TResource>(string path) where TResource : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner<TResource>(path);
        }

        internal TResource? TraverseInner<TResource>(string path) where TResource : class, IEntry
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            if (path.Length > 0 && path[path.Length - 1] == '/')
            {
                // trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
                path = path.Remove(path.Length - 1, 1);
            }
            return (TResource?)TraverseImpl(path.Split('/'));
        }

        [ExposedApi]
        public IEntry? TryGetEntry(string key)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return TryGetEntryInner(key);
        }

        internal IEntry? TryGetEntryInner(string key)
        {
            return Entries.TryGetValue(key, out var x) ? x : null;
        }

        /// <summary>
        /// Add a new Resource to this Directory
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [ExposedApi]
        public TResource AddResource<TResource>(TResource value) where TResource : IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddResourceInner<TResource>(value);
        }

        internal TResource AddResourceInner<TResource>(TResource value) where TResource : IEntry
        {
            return (TResource)AddResourceImpl(value);
        }

        /// <summary>
        /// Add a new Resource to this Directory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ExposedApi]
        public IEntry AddResource(IEntry value)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddResourceImpl(value);
        }

        private IEntry AddResourceImpl(IEntry value)
        {
            if (Entries.ContainsKey(value.Name))
            {
                throw new Exception($"Directory: adding entry with existing name \"{value.Name}\"");
            }
            Entries.Add(value.Name, value);

            // Set this as the parent of the resource.
            // If the resource is a directory, then add the parent (this) to its list of entries
            value.Parent = this;
            if (value.GetType() == typeof(Directory))
            {
                Directory dir = (Directory)value;
                if (dir.Entries[".."] != null)
                {
                    throw new Exception($"Directory: adding entry \"{value.Name}\" with existing parent (entries cannot exist in multiple directories)");
                }
                dir.Entries[".."] = dir.Parent;
            }

            return Entries[value.Name];
        }

        [ExposedApi]
        public void RemoveEntry(string key)
        {
            using var _ = ApiCallSource.StartExternalCall();

            RemoveEntryInner(key);
        }

        internal void RemoveEntryInner(string key)
        {
            if (key.Equals("") || key.Equals(".") || key.Equals(".."))
            {
                throw new Exception("Cannot remove this \".\" or parent \"..\" from directory entries");
            }

            if (Entries.ContainsKey(key))
            {
                Entries[key].DeleteInner();
                Entries.Remove(key);
            }
        }

        [ExposedApi]
        public bool EntryExists(string key)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return EntryExistsInner(key);
        }

        internal bool EntryExistsInner(string key)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return Entries.ContainsKey(key);
        }

        [ExposedApi]
        public IEntry? this[string name]
        {
            get => TryGetEntry(name);
        }
    }
}
