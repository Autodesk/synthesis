// This file was manually created from cef/include/internal/cef_types.h.

namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_media_sink_device_info_t {
        public cef_string_t ip_address;
        public int port;
        public cef_string_t model_name;
    }
}
