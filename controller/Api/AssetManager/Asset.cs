﻿using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using Directory = SynthesisAPI.VirtualFileSystem.Directory;

namespace SynthesisAPI.AssetManager
{
    /// <summary>
    /// Base class for any read-only data class in the vitual file system
    /// </summary>
    public abstract class Asset : IEntry
    {
        /// <summary>
        /// Initialize Asset data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        internal void Init(string name, Permissions perm, string sourcePath)
        {
            _name = name;
            _permissions = perm;
            _parent = null!;
            SourcePath = sourcePath;
        }

        public string Name => ((IEntry)this).Name;
        protected Permissions Permissions => ((IEntry)this).Permissions;
        public Directory Parent => ((IEntry)this).Parent;

        protected string SourcePath { get; private set; } = "";

        private string _name { get; set; } = "";
        private Permissions _permissions { get; set; }
        private Directory _parent { get; set; }

        string IEntry.Name { get => _name; set => _name = value; }
        Permissions IEntry.Permissions { get => _permissions; set => _permissions = value; }
        Directory IEntry.Parent { get => _parent; set => _parent = value; }

        [ExposedApi]
        public virtual void Delete()
        {
            using var _ = ApiCallSource.StartExternalCall();
            DeleteInner();
        }

        [ExposedApi]
        void IEntry.Delete() {
            using var _ = ApiCallSource.StartExternalCall();
            DeleteInner();
        }

        internal virtual void DeleteInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
        }

        void IEntry.DeleteInner()
        {
            ApiCallSource.AssertAccess(Permissions, Access.Write);
        }

        public abstract IEntry Load(byte[] data); // TODO make internal?
    }
}
