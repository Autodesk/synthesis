using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        /// <param name="sourcePath"></param>
        internal void Init(string name, Guid owner, Permissions perm, string sourcePath)
        {
            _name = name;
            _owner = owner;
            _permissions = perm;
            _parent = null!;
            SourcePath = sourcePath;
        }

        public string Name => ((IEntry)this).Name;
        public Guid Owner => ((IEntry)this).Owner;
        public Permissions Permissions => ((IEntry)this).Permissions;
        public Directory Parent => ((IEntry)this).Parent;

        public string SourcePath { get; private set; }

        private string _name { get; set; }
        private Guid _owner { get; set; }
        private Permissions _permissions { get; set; }
        private Directory _parent { get; set; }

        string IEntry.Name { get => _name; set => _name = value; }
        Guid IEntry.Owner { get => _owner; set => _owner = value; }
        Permissions IEntry.Permissions { get => _permissions; set => _permissions = value; }
        Directory IEntry.Parent { get => _parent; set => _parent = value; }

        void IEntry.Delete() { } // Doesn't do anything by default 

        public abstract IEntry Load(byte[] data);
    }
}
