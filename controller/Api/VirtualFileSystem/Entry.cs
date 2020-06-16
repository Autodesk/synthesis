using System;

namespace Api.VirtualFileSystem
{
    public abstract class Entry : IResource
    {
        public string Name { get; }
        public abstract Guid Owner { get; }
        public abstract Permissions Permissions { get; }

        public abstract void Delete();
    }
}
