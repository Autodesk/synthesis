using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Synthesis.CEF.Platform.Windows {
    public static class NativeMethods {
        [DllImport("Kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
