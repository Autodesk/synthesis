using System;

namespace SynthesisAPI.Utilities
{
    public class SynthesisException : Exception
    {
        public SynthesisException() { }
        public SynthesisException(string message) : base(message) { }
        public SynthesisException(string message, Exception inner) : base(message, inner) { }
    }
}
