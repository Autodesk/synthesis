﻿// This file was manually created from cef/include/internal/cef_types.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal struct cef_composition_underline_t {
        public cef_range_t range;
        public uint color;
        public uint background_color;
        public int thick;
        public CefCompositionUnderlineStyle style;
    }
}
