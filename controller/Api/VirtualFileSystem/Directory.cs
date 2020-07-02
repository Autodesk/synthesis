using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// A wrapper for a dictionary of entries that gives structure to the virtual file system
    /// </summary>
    public sealed class Directory : IEntry
    {
        /// <summary>
        /// Initialize Entry data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="perm"></param>
        private void Init(string name, Permissions perm)
        {
            _name = name;
            _permissions = perm;
            _parent = null!;
        }

        public string Name => ((IEntry)this).Name;
        public Permissions Permissions => ((IEntry)this).Permissions;
        public Directory Parent => ((IEntry)this).Parent;

        private string _name { get; set; }
        private Permissions _permissions { get; set; }
        private Directory _parent { get; set; }

        string IEntry.Name { get => _name; set => _name = value; }
        Permissions IEntry.Permissions { get => _permissions; set => _permissions = value; }
        Directory IEntry.Parent { get => _parent; set => _parent = value; }

        internal Dictionary<string, IEntry> Entries;

        public static readonly char DirectorySeparatorChar = '/';

        public Directory(string name, Permissions perm)
        {
            Init(name, perm);
            Entries = new Dictionary<string, IEntry> {{".", this}, {"..", null!}};
        }

        [ExposedApi]
        void IEntry.Delete()
        {
            using var _ = ApiCallSource.StartExternalCall();
            DeleteInner();
        }

        [ExposedApi]
        public void Delete()
        {
            using var _ = ApiCallSource.StartExternalCall();

            DeleteInner();
        }

        internal void DeleteInner() {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            foreach (var e in Entries)
            {
                if (e.Key != "" && e.Key != "." && e.Key != "..")
                {
                    e.Value.DeleteInner();
                }
            }
            if (Parent != null)
            {
                Parent.RemoveEntryInner(Name);
            }
        }

        void IEntry.DeleteInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            DeleteInner();
        }

        [ExposedApi]
        public static string MakePath(params string[] subpaths)
        {
            return String.Join(DirectorySeparatorChar.ToString(), subpaths);
        }

        internal static (string, string[]) GetTopDirectory(string[] paths)
        {
            if(paths.Length == 0)
            {
                throw new DirectroyExpection();
            }
            string target = paths[0];
            paths = paths.Skip(1).ToArray();
            return (target, paths);
        }

        internal static string[] SplitPath(string path)
        {
            if (path.Length > 0 && path[path.Length - 1] == DirectorySeparatorChar)
            {
                // trim the last slash? (ex: "/modules/sample_module/" -> "/modules/sample_module")
                path = path.Remove(path.Length - 1, 1);
            }
            return path.Split(DirectorySeparatorChar);
        }

        private IEntry? TraverseImpl(string[] subpaths) // TODO rework using TDD
        {
            if (subpaths.Length == 0)
            {
                return null;
            }

            string target;
            (target, subpaths) = GetTopDirectory(subpaths);

            var next = TryGetEntryInner(target);

            if (subpaths.Length == 0)
            {
                return next;
            }

            if (next == null)
            {
                return null;
            }

            if (next is Directory directory)
            {
                return directory.TraverseImpl(subpaths);
            }

            return null;
        }

        /// <summary>
        /// Traverse this Directory and subdirectories to a entry given a file path
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

        [ExposedApi]
        public TEntry? Traverse<TEntry>(string[] subpaths) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner<TEntry>(subpaths);
        }


        internal TEntry? TraverseInner<TEntry>(string[] subpaths) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return (TEntry?)TraverseImpl(subpaths);
        }

        /// <summary>
        /// Traverse this Directory and subdirectories to an entry given a file path
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
            return TraverseImpl(SplitPath(path));
        }

        [ExposedApi]
        public TEntry? Traverse<TEntry>(string path) where TEntry : class, IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return TraverseInner<TEntry>(path);
        }

        internal TEntry? TraverseInner<TEntry>(string path) where TEntry : class, IEntry
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return (TEntry?)TraverseImpl(SplitPath(path));
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
        /// Add a new entry to this Directory
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [ExposedApi]
        public TEntry AddEntry<TEntry>(TEntry value) where TEntry : IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddEntryInner(value);
        }

        internal TEntry AddEntryInner<TEntry>(TEntry value) where TEntry : IEntry
        {
            return (TEntry)AddEntryImpl(value);
        }

        /// <summary>
        /// Add a new entry to this Directory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ExposedApi]
        public IEntry AddEntry(IEntry value)
        {
            using var _ = ApiCallSource.StartExternalCall();

            return AddEntryImpl(value);
        }

        private IEntry AddEntryImpl(IEntry value)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            if (value.Name.Equals(""))
            {
                throw new DirectroyExpection("Directory: adding entry with empty name");
            }
            if (Entries.ContainsKey(value.Name))
            {
                throw new DirectroyExpection($"Directory: adding entry with existing name \"{value.Name}\"");
            }
            Entries.Add(value.Name, value);

            // Set this as the parent of the entry.
            // If the entry is a directory, then add the parent (this) to its list of entries
            value.Parent = this;
            if (value is Directory dir)
            {
                if (dir.Entries[".."] != null)
                {
                    throw new DirectroyExpection($"Directory: adding entry \"{value.Name}\" with existing parent (entries cannot exist in multiple directories)");
                }
                dir.Entries[".."] = dir.Parent;
            }

            return Entries[value.Name];
        }

        [ExposedApi]
        public void DeleteEntry(string key)
        {
            using var _ = ApiCallSource.StartExternalCall();

            DeleteEntryInner(key);
        }

        internal void DeleteEntryInner(string key)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            if (key.Equals("") || key.Equals(".") || key.Equals(".."))
            {
                throw new DirectroyExpection("Cannot remove this \".\" or parent \"..\" from directory entries");
            }

            if (Entries.ContainsKey(key))
            {
                Entries[key].DeleteInner();
            }
        }

        [ExposedApi]
        public IEntry? RemoveEntry(string key)
        {
            using var _ = ApiCallSource.StartExternalCall();
            return RemoveEntryInner(key);
        }

        internal IEntry? RemoveEntryInner(string key)
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
            if (key.Equals("") || key.Equals(".") || key.Equals(".."))
            {
                throw new DirectroyExpection("Cannot remove this \".\" or parent \"..\" from directory entries");
            }

            if (Entries.ContainsKey(key))
            {
                var entry = Entries[key];
                entry.Parent = null;
                Entries.Remove(key);
                return entry;
            }
            return null;
        }

        [ExposedApi]
        public bool EntryExists<TEntry>(string key) where TEntry : IEntry
        {
            using var _ = ApiCallSource.StartExternalCall();
            return EntryExistsInner(key);
        }

        internal bool EntryExistsInner<TEntry>(string key) where TEntry : IEntry
        {
            ApiCallSource.AssertAccess(Permissions, Access.Read);
            return Entries.ContainsKey(key) && Entries[key] is TEntry;
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
