using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public abstract class Resource
    {
        public virtual string Name { get; protected set;  }

        public virtual Guid Owner { get; protected set;  }

        public virtual Permissions Permissions { get; protected set;  }

        public virtual void Init(string name, Guid owner, Permissions perm)
        {
            Name = name;
            Owner = owner;
            Permissions = perm;
        }

        public virtual void Delete() { } // Doesn't do anything by default 
    }
}
