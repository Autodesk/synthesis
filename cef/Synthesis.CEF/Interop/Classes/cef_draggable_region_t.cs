// This file was manually created from cef/include/internal/cef_types.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_draggable_region_t {
        public cef_rect_t bounds;
        public int draggable;
    }
}
