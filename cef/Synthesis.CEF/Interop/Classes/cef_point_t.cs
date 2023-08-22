// This file was manually created from cef/include/internal/cef_types_geometry.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_point_t {
        public int x;
        public int y;

        public cef_point_t(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
}
