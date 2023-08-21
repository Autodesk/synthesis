//
// This file was manually created from cef/include/internal/cef_types.h.
//
namespace Synthesis.CEF.Interop {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_audio_parameters_t {
        public CefChannelLayout channel_layout;
        public int sample_rate;
        public int frames_per_buffer;
    }
}
