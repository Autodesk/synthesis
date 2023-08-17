using System;
using System.Runtime.InteropServices;

namespace Synthesis.CEF.Interop {
    internal unsafe struct cef_main_args_t_windows {
        public IntPtr instance;
        private static int _size;

        static cef_main_args_t_windows() {
            _size = Marshal.SizeOf(typeof(cef_main_args_t_windows));
        }

        public static cef_main_args_t_windows *Alloc() {
            var ptr = (cef_main_args_t_windows *) Marshal.AllocHGlobal(_size);
            *ptr    = new cef_main_args_t_windows();
            return ptr;
        }

        public static void Free(cef_main_args_t_windows *ptr) {
            Marshal.FreeHGlobal((IntPtr) ptr);
        }
    }

    // TODO: Going to need to be different for posix
}
