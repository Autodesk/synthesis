using System;

namespace SynthesisAPI.VirtualFileSystem
{
    public class SynthesisExpection : Exception
    {
        public SynthesisExpection() { }
        public SynthesisExpection(string message) : base(message) { }
        public SynthesisExpection(string message, Exception inner) : base(message, inner) { }
    }
}
