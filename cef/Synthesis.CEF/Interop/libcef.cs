using System;
using System.Runtime.InteropServices;

namespace Synthesis.CEF.Interop {
    internal static unsafe partial class libcef {
        internal const string DllName                 = "libcef";
        internal const CallingConvention CEF_CALL     = CallingConvention.Cdecl;
        internal const CallingConvention CEF_CALLBACK = CallingConvention.Winapi;

        // Specifies the PACK value for auto generated classes from CefGlue.
        internal const int ALIGN = 0;

        [DllImport(DllName, EntryPoint = "cef_api_hash", CallingConvention = CEF_CALL)]
        public static extern sbyte *api_hash(int entry);
    }
}
