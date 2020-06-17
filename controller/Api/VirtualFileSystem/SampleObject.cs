using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public class SampleObject : Entry
    {
        public SampleObject(Guid owner, Permissions perm)
        {
            Owner = owner;
            Permissions = perm;
        }

        public new string Name { get; }

        public override void Delete()
        {
            // TODO
        }

        // private data
    }
}
