// This file was manually created from cef/include/internal/cef_types.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_screen_info_t {
        public float device_scale_factor;
        public int depth;
        public int depth_per_component;
        public int is_monochrome;
        public cef_rect_t rect;
        public cef_rect_t available_rect;
    }
}
