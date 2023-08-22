// This file was manually created from cef/include/internal/cef_types_geometry.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_rect_t {
        public int x;
        public int y;
        public int width;
        public int height;

        public cef_rect_t(int x, int y, int width, int height) {
            this.x      = x;
            this.y      = y;
            this.width  = width;
            this.height = height;
        }
    }
}
