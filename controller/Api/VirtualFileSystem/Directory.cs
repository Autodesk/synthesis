using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A wrapper for a dictionary of resources that gives structure to the virtual file system
    /// </summary>
    public class Directory : Resource
    {
        public Directory(string name) : this(name, Guid.Empty, Permissions.Private) { }

        public Directory(string name, Guid owner, Permissions perm)
        {
            Init(name, owner, perm);
            Entries = new Dictionary<string, IResource>();
            Entries.Add("", this);
            Entries.Add(".", this);
            Entries.Add("..", null);
        }

        internal Dictionary<string, IResource> Entries;
        
        /// <summary>
        /// Traverse this Directory and subdirectories to a resource given a file path
        /// </summary>
        /// <param name="subpaths"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Traverse this Directory and subdirectories to a resource given a file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IResource Traverse(string path)
        {
            return Traverse(path.Split('/')); // TODO should we trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
        }

        public IResource TryGetEntry(string key) // TODO make private?
        {
            return Entries.TryGetValue(key, out var x) ? x : null;
        }

        public IResource GetEntry(string key) // TODO remove?
        {
            if (!Entries.ContainsKey(key))
            {
                throw new Exception();
            }
            return Entries[key];
        }

        /// <summary>
        /// Add a new Resource to this Directory
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public TResource AddEntry<TResource>(TResource value) where TResource : IResource // TODO rename to AddResource?
        {
            return (TResource)AddEntryImpl(value);
        }

        /// <summary>
        /// Add a new Resource to this Directory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

            // Set this as the parent of the resource.
            // If the resource is a directory, then add the parent (this) to its list of entries
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

        public void RemoveEntry(string key) => RemoveEntry(key, Guid.Empty);

        public void RemoveEntry(string key, Guid guid)
        {
            if (key.Equals("") || key.Equals(".") || key.Equals(".."))
            {
                throw new Exception();
            }

            if (Entries.ContainsKey(key))
            {
                if (Entries[key].Permissions == Permissions.PublicWrite || Entries[key].Owner == guid)
                {
                    Entries[key].Delete();
                    Entries.Remove(key);
                }
                else
                    throw new Exception(string.Format("\"{0}\" doesn't have permission to delete \"{1}\"", guid, key));
            }
        }

        public bool EntryExists(string key)
        {
            return Entries.ContainsKey(key);
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
