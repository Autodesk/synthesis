#[macro_use] extern crate jsonrpc_client_core;
#[macro_use] extern crate lazy_static;

use std::sync::Mutex;
use jsonrpc_client_core::ErrorKind;
use jsonrpc_client_http::HttpTransport;
use jsonrpc_client_http::HttpHandle;
use std::os::raw::c_char;
use serde_json::value::Value;
use serde;
use serde::{Serialize, Deserialize};
use std::ptr::null_mut;
use std::mem::ManuallyDrop;
use std::ffi::{CString, CStr};
use serde::export::Formatter;

#[repr(u64)]
#[derive(Serialize, Deserialize)]
pub enum LogLevel {
    Info = 0,
    Debug = 1,
    Warning = 2,
    Error = 3
}

impl std::fmt::Display for LogLevel {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        match *self {
            LogLevel::Info => write!(f, "Info"),
            LogLevel::Debug => write!(f, "Debug"),
            LogLevel::Warning => write!(f, "Warning"),
            LogLevel::Error => write!(f, "Error"),
        }
    }
}

jsonrpc_client!(pub struct ControllerRpc {
    pub fn log_str(&mut self, message: NativeString, log_level: LogLevel) -> RpcRequest<()>;
    pub fn forward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn backward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn left(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn right(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn up(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn down(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn test(&mut self, val: i32) -> RpcRequest<i32>;
    pub fn set_motor_percent(&mut self, channel: u32, motorIndex: i32, percent: f32) -> RpcRequest<()>;
});

lazy_static! {
    static ref CLIENT : Mutex<ControllerRpc<HttpHandle>> = {
        let transport = HttpTransport::new().standalone().unwrap();
        let transport_handle = transport.handle("http://localhost:5000").unwrap();
        Mutex::new(ControllerRpc::new(transport_handle))
    };
}

/*
macro_rules! print_all {
    ($($args:expr),*) => {
        $(println!("\"{}\"", $args); )*
    }
}
*/

macro_rules! rpc_method_impl {
    ($name:ident($($arg_name:ident: $ty:ty),*) -> $ret_type:ty) => {
        #[no_mangle]
        pub extern "C" fn $name (
            $($arg_name: $ty),*,
            error_code: *mut i64,
            error_message: *mut *const c_char,
            error_data: *mut *const c_char
        ) -> $ret_type {
            let pass_ownership = |s: String| unsafe {
                ManuallyDrop::take(
                    &mut ManuallyDrop::new(CString::new(s.as_bytes()).unwrap()))
                .into_raw()
            };
            match CLIENT.lock().unwrap().$name($($arg_name),*).call() {
                Ok(v) => {
                    if error_code != null_mut() {
                        unsafe { *error_code = 0; }
                    }
                    return v;
                },
                Err(e) => match *e.kind() {
                    ErrorKind::Msg(ref message) => {
                        if error_code != null_mut() {
                            unsafe { *error_code = -1; }
                        }
                        if error_message != null_mut() {
                            unsafe {
                                *error_message = pass_ownership(message.clone());
                            }
                        }
                    },
                    ErrorKind::TransportError => {
                        if error_code != null_mut() {
                            unsafe { *error_code = -1; }
                        }
                        if error_message != null_mut() {
                            unsafe {
                                *error_message = pass_ownership("Error in the underlying transport layer".to_string());
                            }
                        }
                    },
                    ErrorKind:: SerializeError => {
                        if error_code != null_mut() {
                            unsafe { *error_code = -1; }
                        }
                        if error_message != null_mut() {
                            unsafe {
                                *error_message = pass_ownership("Error while serializing method parameters".to_string());
                            }
                        }
                    },
                    ErrorKind::ResponseError(ref message) => {
                        if error_code != null_mut() {
                            unsafe { *error_code = -1; }
                        }
                        if error_message != null_mut() {
                            unsafe {
                                *error_message = pass_ownership("Error while deserializing or parsing the response data".to_string());
                            }
                        }
                        if error_data != null_mut() {
                            unsafe {
                                *error_data = pass_ownership(message.to_string().clone());
                            }
                        }
                    },
                    ErrorKind::JsonRpcError(ref json_rpc_error) => {
                        if error_code != null_mut() {
                            unsafe { *error_code = json_rpc_error.code.code(); }
                        }
                        if error_message != null_mut() {
                            unsafe {
                                *error_message = pass_ownership(json_rpc_error.message.clone());
                            }
                        }
                        if error_data != null_mut() {
                            if let Some(value) = &json_rpc_error.data {
                                if let Value::String(data_str) = value {
                                    unsafe { *error_data = pass_ownership(data_str.clone()); }
                                }
                            }
                        }
                    },
                    _ => {
                        if error_code != null_mut() {
                            unsafe { *error_code = -1; }
                        }
                    }
                }
            };
            return Default::default();
        }
    }
}

macro_rules! rpc_method {
    ($name:ident($($arg_name:ident: $ty:ty),*)) => {
        rpc_method_impl!($name($($arg_name: $ty),*) -> $ret_type);
    };
    ($name:ident($($arg_name:ident: $ty:ty),*) -> $ret_type:ty) => {
        rpc_method_impl!($name($($arg_name: $ty),*) -> $ret_type);
    };
}

macro_rules! rpc_methods {
    {
        $($name:ident($($arg_name:ident: $ty:ty),*) -> $ret_type:ty)*
    } => {
        $(rpc_method!($name($($arg_name: $ty),*) -> $ret_type);)*
    }
}

#[repr(transparent)]
pub struct NativeString(*const c_char);

impl std::fmt::Display for NativeString {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        if !self.0.is_null() {
            let c_str = unsafe { CStr::from_ptr(self.0) };
            return write!(f, "{}", c_str.to_str().unwrap());
        }
        Err(std::fmt::Error)
    }
}

impl serde::Serialize for NativeString {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error> where S: serde::Serializer {
        let c_str = unsafe { CStr::from_ptr(self.0) };
        serializer.serialize_str(c_str.to_str().unwrap())
    }
}

rpc_methods!{
    log_str(message: NativeString, log_level: LogLevel) -> ()
    forward(channel: u32, distance: f64) -> ()
    backward(channel: u32, distance: f64) -> ()
    left(channel: u32, distance: f64) -> ()
    right(channel: u32, distance: f64) -> ()
    up(channel: u32, distance: f64) -> ()
    down(channel: u32, distance: f64) -> ()
    test(val: i32) -> i32
    set_motor_percent(channel: u32, motorIndex: i32, percent: f32) -> ()
}