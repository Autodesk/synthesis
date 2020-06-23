using SynthesisAPI.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.AssetManager
{
    public abstract class Asset : IAsset
    {
        public void Init(string name, Guid owner, Permissions perm)
        {
            _name = name;
            _owner = owner;
            _permissions = perm;
        }

        public string Name => ((IResource)this).Name;

        public Guid Owner => ((IResource)this).Owner;

        public Permissions Permissions => ((IResource)this).Permissions;

        public Directory Parent => ((IResource)this).Parent;

        private string _name { get; set; }
        private Guid _owner { get; set; }
        private Permissions _permissions { get; set; }
        private Directory _parent { get; set; }

        string IResource.Name { get => _name; set => _name = value; }
        Guid IResource.Owner { get => _owner; set => _owner = value; }
        Permissions IResource.Permissions { get => _permissions; set => _permissions = value; }
        Directory IResource.Parent { get => _parent; set => _parent = value; }

        void IResource.Delete()
        {
            
        }

        IResource IAsset.Load(byte[] data)
        {
            return Load(data);
        }

        public abstract IResource Load(byte[] data);
    }
}
