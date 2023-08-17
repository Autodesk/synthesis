using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Synthesis.CEF.Interop;

using Windows = Synthesis.CEF.Platform.Windows;

namespace Synthesis.CEF {
    public unsafe class CEFMainArgs {
        private readonly string[] _args;
        private IntPtr _argvBlock;

        public CEFMainArgs(string[] args) {
            _args      = args;
            _argvBlock = IntPtr.Zero;
        }

        internal cef_main_args_t_windows *ToNative() {
            var ptr       = cef_main_args_t_windows.Alloc();
            ptr->instance = Windows.NativeMethods.GetModuleHandle(null);
            return ptr;
        }

        // Stolen from CefGlue, it is noted that this needed to be re-implemented
        private IntPtr MarshallArgcArgvBlock(string[] args) {
            var sizeOfArray = sizeof(IntPtr) * (args.Length + 1);

            foreach (var arg in args) {
                size += 1 + Encoding.UTF8.GetByteCount(arg ?? "");
            }

            byte **argv = (byte **) Marshal.AllocHGlobal(sizeOfArray);
            byte *data  = (byte *) argv + sizeOfArray;

            for (var i = 0; i < args.Length; i++) {
                argv[i]   = data;
                var bytes = Encoding.UTF8.GetBytes(args[i]);
                Marshal.Copy(bytes, 0, (IntPtr) data, bytes.Length);
                data += bytes.Length;
                data[0] = 0;
                data++;
            }

            argv[args.Length] = (byte *) 0;
            return (IntPtr) argv;
        }

        private void Free(cef_main_args_t_windows *ptr) {
            cef_main_args_t_windows.Free(ptr);
        }
    }
}
