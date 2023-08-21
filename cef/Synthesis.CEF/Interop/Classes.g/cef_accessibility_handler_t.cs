// This file was autogenerated using CefGlue's cef interop header layer generator script.

//
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
//
namespace Synthesis.CEF.Interop {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
    internal unsafe struct cef_accessibility_handler_t {
        internal cef_base_ref_counted_t _base;
        internal IntPtr _on_accessibility_tree_change;
        internal IntPtr _on_accessibility_location_change;

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void add_ref_delegate(cef_accessibility_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int release_delegate(cef_accessibility_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_one_ref_delegate(cef_accessibility_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_at_least_one_ref_delegate(cef_accessibility_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void on_accessibility_tree_change_delegate(
            cef_accessibility_handler_t *self, cef_value_t *value);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void on_accessibility_location_change_delegate(
            cef_accessibility_handler_t *self, cef_value_t *value);

        private static int _sizeof;

        static cef_accessibility_handler_t() {
            _sizeof = Marshal.SizeOf(typeof(cef_accessibility_handler_t));
        }

        internal static cef_accessibility_handler_t *Alloc() {
            var ptr          = (cef_accessibility_handler_t *) Marshal.AllocHGlobal(_sizeof);
            *ptr             = new cef_accessibility_handler_t();
            ptr->_base._size = (UIntPtr) _sizeof;
            return ptr;
        }

        internal static void Free(cef_accessibility_handler_t *ptr) {
            Marshal.FreeHGlobal((IntPtr) ptr);
        }
    }
}
