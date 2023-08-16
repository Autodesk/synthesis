using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Synthesis.CEF.Interop;

namespace Synthesis.CEF {
    public static class CEFRuntime {
        public static readonly CefRuntimePlatform Platform;

        private static bool _loaded = false;

        static CefRuntime() {
            var platformID = Environment.OSVersion.Platform;

            if (platformID == PlatformID.MacOSX) {
                Platform = CefRuntimePlatform.Mac;
            } else {
                int p = (int) platformID;

                if ((p == 4) || (p == 128)) {
                    Platform = IsRunningOnMac() ? CefRuntimePlatform.Mac : CefRuntimePlatform.Linux;
                } else {
                    Platform = CefRuntimePlatform.Windows;
                }
            }
        }

        // Stolen from CefGlue, very hacky should look for another solution.
        private static bool IsRunningOnMac() {
            IntPtr buffer = IntPtr.Zero;

            try {
                buffer = Marshal.AllocHGlobal(8192);

                if (uname(buffer) == 0) {
                    string os = Marshal.PtrToStringAnsi(buffer);

                    if (os == "Darwin") {
                        return true;
                    }
                }
            } catch {}
            finally {
                if (buffer != IntPtr.Zero) {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            return false;
        }

        [DllImport("libc")]
        private static extern int uname(IntPtr buffer);

        public static void Load() {
            if (_loaded) {
                return;
            }

            string? actual;

            // Check cef api hash by platform
            try {
                var actual_ = libcef.api_hash(0);
                actual = actual_ != null ? new string(actual_) : null;
            } catch (EntryPointNotFoundException) {
                throw new CEFCallMismatch("cef_api_hash call not found");
            }

            if (string.IsNullOrEmpty(actual)) {
                throw new CEFCallMismatch("cef_api_hash call returned null or empty");
            }

            string expected;

            switch (CefRuntime.Platform) {
                case CefRuntimePlatform.Windows:
                    expected = libcef.CEF_API_HASH_PLATFORM_WIN;
                    break;
                case CefRuntimePlatform.Mac:
                    expected = libcef.CEF_API_HASH_PLATFORM_MACOSX;
                    break;
                case CefRuntimePlatform.Linux:
                    expected = libcef.CEF_API_HASH_PLATFORM_LINUX;
                    break;
            }

            if (actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) {
                throw new CEFCallMismatch($"cef_api_hash call returned {actual} but expected {expected}");
            }

            _loaded = true;
        }
    }

    public enum CefRuntimePlatform {
        Windows,
        Mac,
        Linux
    }
}
