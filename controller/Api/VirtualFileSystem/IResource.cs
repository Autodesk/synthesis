using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public interface IResource
    {
        public string Name { get; internal set; }

        public Guid Owner { get; internal set; }

        public Permissions Permissions { get; internal set;  }

        public Directory Parent { get; internal set; }

        public void Delete();
    }
}
