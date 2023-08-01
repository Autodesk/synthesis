using System.Runtime.InteropServices;
using System;

namespace Synthesis.CEF {
    public static class CEF {
        internal const string DllName = "libsynthesis_cef_wrapper";

        [DllImport(DllName, EntryPoint = "RunCefInterop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RunCef();
    }
}
