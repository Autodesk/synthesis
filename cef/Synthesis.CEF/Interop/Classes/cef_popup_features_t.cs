// This file was manually created from cef/include/internal/cef_types.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_popup_features_t {
        public int x;
        public int xSet;
        public int y;
        public int ySet;
        public int width;
        public int widthSet;
        public int height;
        public int heightSet;

        public int menuBarVisible;
        public int statusBarVisible;
        public int toolBarVisible;
        public int scrollbarsVisible;
    }
}
