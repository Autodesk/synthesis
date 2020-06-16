using System;

namespace Api.VirtualFileSystem
{
    public class SampleObject : Entry
    {
        public SampleObject(Guid owner, Permissions perm)
        {
            Owner = owner;
            Permissions = perm;
        }

        public new string Name { get; }

        public override Guid Owner { get; }

        public override Permissions Permissions { get; }

        public override void Delete()
        {
            // TODO
        }

        // private data
    }
}
