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
    internal unsafe struct cef_v8interceptor_t {
        internal cef_base_ref_counted_t _base;
        internal IntPtr _get_byname;
        internal IntPtr _get_byindex;
        internal IntPtr _set_byname;
        internal IntPtr _set_byindex;

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void add_ref_delegate(cef_v8interceptor_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int release_delegate(cef_v8interceptor_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_one_ref_delegate(cef_v8interceptor_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_at_least_one_ref_delegate(cef_v8interceptor_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int get_byname_delegate(cef_v8interceptor_t *self, cef_string_t *name, cef_v8value_t *@object,
            cef_v8value_t **retval, cef_string_t *exception);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int get_byindex_delegate(cef_v8interceptor_t *self, int index, cef_v8value_t *@object,
            cef_v8value_t **retval, cef_string_t *exception);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int set_byname_delegate(cef_v8interceptor_t *self, cef_string_t *name, cef_v8value_t *@object,
            cef_v8value_t *value, cef_string_t *exception);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int set_byindex_delegate(cef_v8interceptor_t *self, int index, cef_v8value_t *@object,
            cef_v8value_t *value, cef_string_t *exception);

        private static int _sizeof;

        static cef_v8interceptor_t() {
            _sizeof = Marshal.SizeOf(typeof(cef_v8interceptor_t));
        }

        internal static cef_v8interceptor_t *Alloc() {
            var ptr          = (cef_v8interceptor_t *) Marshal.AllocHGlobal(_sizeof);
            *ptr             = new cef_v8interceptor_t();
            ptr->_base._size = (UIntPtr) _sizeof;
            return ptr;
        }

        internal static void Free(cef_v8interceptor_t *ptr) {
            Marshal.FreeHGlobal((IntPtr) ptr);
        }
    }
}
