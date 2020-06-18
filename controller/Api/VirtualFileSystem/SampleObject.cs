using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public class SampleObject : Entry
    {
        public SampleObject(string name, Guid owner, Permissions perm)
        {
            Init(name, owner, perm);
        }

        public override void Delete()
        {
            // TODO
        }

        // private data
    }
}
