using System;

namespace Api.VirtualFileSystem
{
    public interface IResource
    {
        public string Name { get; }
        public Guid Owner { get; }
        public Permissions Permissions { get; }
        void Delete();
    }
}
