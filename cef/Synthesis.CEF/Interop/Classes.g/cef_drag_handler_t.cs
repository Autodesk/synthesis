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
    internal unsafe struct cef_drag_handler_t {
        internal cef_base_ref_counted_t _base;
        internal IntPtr _on_drag_enter;
        internal IntPtr _on_draggable_regions_changed;

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void add_ref_delegate(cef_drag_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int release_delegate(cef_drag_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_one_ref_delegate(cef_drag_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int has_at_least_one_ref_delegate(cef_drag_handler_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate int on_drag_enter_delegate(
            cef_drag_handler_t *self, cef_browser_t *browser, cef_drag_data_t *dragData, CefDragOperationsMask mask);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        internal delegate void on_draggable_regions_changed_delegate(cef_drag_handler_t *self, cef_browser_t *browser,
            cef_frame_t *frame, UIntPtr regionsCount, cef_draggable_region_t *regions);

        private static int _sizeof;

        static cef_drag_handler_t() {
            _sizeof = Marshal.SizeOf(typeof(cef_drag_handler_t));
        }

        internal static cef_drag_handler_t *Alloc() {
            var ptr          = (cef_drag_handler_t *) Marshal.AllocHGlobal(_sizeof);
            *ptr             = new cef_drag_handler_t();
            ptr->_base._size = (UIntPtr) _sizeof;
            return ptr;
        }

        internal static void Free(cef_drag_handler_t *ptr) {
            Marshal.FreeHGlobal((IntPtr) ptr);
        }
    }
}
