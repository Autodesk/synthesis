using System;

namespace Synthesis.CEF {
    /// <summary>
    /// Thrown when an expected CEF function call is not found. Expected cause is an incompatible CEF version.
    /// </summary>
    public class CEFCallMismatch : Exception {
        public CEFCallMismatch(string message) : base(message) {}
    }
}
