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
    internal unsafe struct cef_server_t {
        internal cef_base_ref_counted_t _base;
        internal IntPtr _get_task_runner;
        internal IntPtr _shutdown;
        internal IntPtr _is_running;
        internal IntPtr _get_address;
        internal IntPtr _has_connection;
        internal IntPtr _is_valid_connection;
        internal IntPtr _send_http200response;
        internal IntPtr _send_http404response;
        internal IntPtr _send_http500response;
        internal IntPtr _send_http_response;
        internal IntPtr _send_raw_data;
        internal IntPtr _close_connection;
        internal IntPtr _send_web_socket_message;

        // CreateServer
        [DllImport(libcef.DllName, EntryPoint = "cef_server_create", CallingConvention = libcef.CEF_CALL)]
        public static extern void create(
            cef_string_t *address, ushort port, int backlog, cef_server_handler_t *handler);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void add_ref_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int release_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int has_one_ref_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int has_at_least_one_ref_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate cef_task_runner_t *get_task_runner_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void shutdown_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int is_running_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate cef_string_userfree *get_address_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int has_connection_delegate(cef_server_t *self);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate int is_valid_connection_delegate(cef_server_t *self, int connection_id);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_http200response_delegate(
            cef_server_t *self, int connection_id, cef_string_t *content_type, void *data, UIntPtr data_size);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_http404response_delegate(cef_server_t *self, int connection_id);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_http500response_delegate(
            cef_server_t *self, int connection_id, cef_string_t *error_message);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_http_response_delegate(cef_server_t *self, int connection_id, int response_code,
            cef_string_t *content_type, long content_length, cef_string_multimap *extra_headers);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_raw_data_delegate(
            cef_server_t *self, int connection_id, void *data, UIntPtr data_size);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void close_connection_delegate(cef_server_t *self, int connection_id);

        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
#if !DEBUG
        [SuppressUnmanagedCodeSecurity]
#endif
        private delegate void send_web_socket_message_delegate(
            cef_server_t *self, int connection_id, void *data, UIntPtr data_size);

        // AddRef
        private static IntPtr _p0;
        private static add_ref_delegate _d0;

        public static void add_ref(cef_server_t *self) {
            add_ref_delegate d;
            var p = self->_base._add_ref;
            if (p == _p0) {
                d = _d0;
            } else {
                d = (add_ref_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(add_ref_delegate));
                if (_p0 == IntPtr.Zero) {
                    _d0 = d;
                    _p0 = p;
                }
            }
            d(self);
        }

        // Release
        private static IntPtr _p1;
        private static release_delegate _d1;

        public static int release(cef_server_t *self) {
            release_delegate d;
            var p = self->_base._release;
            if (p == _p1) {
                d = _d1;
            } else {
                d = (release_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(release_delegate));
                if (_p1 == IntPtr.Zero) {
                    _d1 = d;
                    _p1 = p;
                }
            }
            return d(self);
        }

        // HasOneRef
        private static IntPtr _p2;
        private static has_one_ref_delegate _d2;

        public static int has_one_ref(cef_server_t *self) {
            has_one_ref_delegate d;
            var p = self->_base._has_one_ref;
            if (p == _p2) {
                d = _d2;
            } else {
                d = (has_one_ref_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(has_one_ref_delegate));
                if (_p2 == IntPtr.Zero) {
                    _d2 = d;
                    _p2 = p;
                }
            }
            return d(self);
        }

        // HasAtLeastOneRef
        private static IntPtr _p3;
        private static has_at_least_one_ref_delegate _d3;

        public static int has_at_least_one_ref(cef_server_t *self) {
            has_at_least_one_ref_delegate d;
            var p = self->_base._has_at_least_one_ref;
            if (p == _p3) {
                d = _d3;
            } else {
                d = (has_at_least_one_ref_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(has_at_least_one_ref_delegate));
                if (_p3 == IntPtr.Zero) {
                    _d3 = d;
                    _p3 = p;
                }
            }
            return d(self);
        }

        // GetTaskRunner
        private static IntPtr _p4;
        private static get_task_runner_delegate _d4;

        public static cef_task_runner_t *get_task_runner(cef_server_t *self) {
            get_task_runner_delegate d;
            var p = self->_get_task_runner;
            if (p == _p4) {
                d = _d4;
            } else {
                d = (get_task_runner_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(get_task_runner_delegate));
                if (_p4 == IntPtr.Zero) {
                    _d4 = d;
                    _p4 = p;
                }
            }
            return d(self);
        }

        // Shutdown
        private static IntPtr _p5;
        private static shutdown_delegate _d5;

        public static void shutdown(cef_server_t *self) {
            shutdown_delegate d;
            var p = self->_shutdown;
            if (p == _p5) {
                d = _d5;
            } else {
                d = (shutdown_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(shutdown_delegate));
                if (_p5 == IntPtr.Zero) {
                    _d5 = d;
                    _p5 = p;
                }
            }
            d(self);
        }

        // IsRunning
        private static IntPtr _p6;
        private static is_running_delegate _d6;

        public static int is_running(cef_server_t *self) {
            is_running_delegate d;
            var p = self->_is_running;
            if (p == _p6) {
                d = _d6;
            } else {
                d = (is_running_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(is_running_delegate));
                if (_p6 == IntPtr.Zero) {
                    _d6 = d;
                    _p6 = p;
                }
            }
            return d(self);
        }

        // GetAddress
        private static IntPtr _p7;
        private static get_address_delegate _d7;

        public static cef_string_userfree *get_address(cef_server_t *self) {
            get_address_delegate d;
            var p = self->_get_address;
            if (p == _p7) {
                d = _d7;
            } else {
                d = (get_address_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(get_address_delegate));
                if (_p7 == IntPtr.Zero) {
                    _d7 = d;
                    _p7 = p;
                }
            }
            return d(self);
        }

        // HasConnection
        private static IntPtr _p8;
        private static has_connection_delegate _d8;

        public static int has_connection(cef_server_t *self) {
            has_connection_delegate d;
            var p = self->_has_connection;
            if (p == _p8) {
                d = _d8;
            } else {
                d = (has_connection_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(has_connection_delegate));
                if (_p8 == IntPtr.Zero) {
                    _d8 = d;
                    _p8 = p;
                }
            }
            return d(self);
        }

        // IsValidConnection
        private static IntPtr _p9;
        private static is_valid_connection_delegate _d9;

        public static int is_valid_connection(cef_server_t *self, int connection_id) {
            is_valid_connection_delegate d;
            var p = self->_is_valid_connection;
            if (p == _p9) {
                d = _d9;
            } else {
                d = (is_valid_connection_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(is_valid_connection_delegate));
                if (_p9 == IntPtr.Zero) {
                    _d9 = d;
                    _p9 = p;
                }
            }
            return d(self, connection_id);
        }

        // SendHttp200Response
        private static IntPtr _pa;
        private static send_http200response_delegate _da;

        public static void send_http200response(
            cef_server_t *self, int connection_id, cef_string_t *content_type, void *data, UIntPtr data_size) {
            send_http200response_delegate d;
            var p = self->_send_http200response;
            if (p == _pa) {
                d = _da;
            } else {
                d = (send_http200response_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(send_http200response_delegate));
                if (_pa == IntPtr.Zero) {
                    _da = d;
                    _pa = p;
                }
            }
            d(self, connection_id, content_type, data, data_size);
        }

        // SendHttp404Response
        private static IntPtr _pb;
        private static send_http404response_delegate _db;

        public static void send_http404response(cef_server_t *self, int connection_id) {
            send_http404response_delegate d;
            var p = self->_send_http404response;
            if (p == _pb) {
                d = _db;
            } else {
                d = (send_http404response_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(send_http404response_delegate));
                if (_pb == IntPtr.Zero) {
                    _db = d;
                    _pb = p;
                }
            }
            d(self, connection_id);
        }

        // SendHttp500Response
        private static IntPtr _pc;
        private static send_http500response_delegate _dc;

        public static void send_http500response(cef_server_t *self, int connection_id, cef_string_t *error_message) {
            send_http500response_delegate d;
            var p = self->_send_http500response;
            if (p == _pc) {
                d = _dc;
            } else {
                d = (send_http500response_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(send_http500response_delegate));
                if (_pc == IntPtr.Zero) {
                    _dc = d;
                    _pc = p;
                }
            }
            d(self, connection_id, error_message);
        }

        // SendHttpResponse
        private static IntPtr _pd;
        private static send_http_response_delegate _dd;

        public static void send_http_response(cef_server_t *self, int connection_id, int response_code,
            cef_string_t *content_type, long content_length, cef_string_multimap *extra_headers) {
            send_http_response_delegate d;
            var p = self->_send_http_response;
            if (p == _pd) {
                d = _dd;
            } else {
                d = (send_http_response_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(send_http_response_delegate));
                if (_pd == IntPtr.Zero) {
                    _dd = d;
                    _pd = p;
                }
            }
            d(self, connection_id, response_code, content_type, content_length, extra_headers);
        }

        // SendRawData
        private static IntPtr _pe;
        private static send_raw_data_delegate _de;

        public static void send_raw_data(cef_server_t *self, int connection_id, void *data, UIntPtr data_size) {
            send_raw_data_delegate d;
            var p = self->_send_raw_data;
            if (p == _pe) {
                d = _de;
            } else {
                d = (send_raw_data_delegate) Marshal.GetDelegateForFunctionPointer(p, typeof(send_raw_data_delegate));
                if (_pe == IntPtr.Zero) {
                    _de = d;
                    _pe = p;
                }
            }
            d(self, connection_id, data, data_size);
        }

        // CloseConnection
        private static IntPtr _pf;
        private static close_connection_delegate _df;

        public static void close_connection(cef_server_t *self, int connection_id) {
            close_connection_delegate d;
            var p = self->_close_connection;
            if (p == _pf) {
                d = _df;
            } else {
                d = (close_connection_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(close_connection_delegate));
                if (_pf == IntPtr.Zero) {
                    _df = d;
                    _pf = p;
                }
            }
            d(self, connection_id);
        }

        // SendWebSocketMessage
        private static IntPtr _p10;
        private static send_web_socket_message_delegate _d10;

        public static void send_web_socket_message(
            cef_server_t *self, int connection_id, void *data, UIntPtr data_size) {
            send_web_socket_message_delegate d;
            var p = self->_send_web_socket_message;
            if (p == _p10) {
                d = _d10;
            } else {
                d = (send_web_socket_message_delegate) Marshal.GetDelegateForFunctionPointer(
                    p, typeof(send_web_socket_message_delegate));
                if (_p10 == IntPtr.Zero) {
                    _d10 = d;
                    _p10 = p;
                }
            }
            d(self, connection_id, data, data_size);
        }
    }
}
