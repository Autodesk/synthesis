using System;

namespace SynthesisAPI.VirtualFileSystem
{
    /// <summary>
    /// Base class for any mutable data class in the vitual file system
    /// </summary>
    public class Resource : IResource
    {
        /// <summary>
        /// Initialize Resource data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="perm"></param>
        internal void Init(string name, Guid owner, Permissions perm)
        {
            _name = name;
            _owner = owner;
            _permissions = perm;
            // Leave _parent as null
        }

        public virtual void Delete() { } // Doesn't do anything by default 

        public string Name => ((IResource)this).Name;
        public Guid Owner => ((IResource)this).Owner;
        public Permissions Permissions => ((IResource)this).Permissions;
        public Directory Parent => ((IResource)this).Parent;

        private string _name { get; set; }
        private Guid _owner { get;  set; }
        private Permissions _permissions { get;  set; }
        private Directory _parent { get; set; }

        string IResource.Name { get => _name; set => _name = value; }
        Guid IResource.Owner { get => _owner; set => _owner = value; }
        Permissions IResource.Permissions { get => _permissions; set => _permissions = value; }
        Directory IResource.Parent { get => _parent; set => _parent = value; }
    }
}
