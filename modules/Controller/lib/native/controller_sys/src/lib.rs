#[macro_use] extern crate jsonrpc_client_core;
#[macro_use] extern crate jsonrpc_client_http;
#[macro_use] extern crate lazy_static;

use std::sync::Mutex;
use jsonrpc_client_core::ErrorKind;
use jsonrpc_client_http::HttpTransport;
use jsonrpc_client_http::HttpHandle;
use std::os::raw::c_char;
use serde_json::value::Value;
use std::ptr::{null, null_mut};
use std::mem::ManuallyDrop;
use std::ffi::CString;

jsonrpc_client!(pub struct ControllerRpc {
    pub fn Forward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Backward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Left(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Right(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Test(&mut self, test: i32) -> RpcRequest<i32>;
});

lazy_static! {
    static ref CLIENT : Mutex<ControllerRpc<HttpHandle>> = {
        let transport = HttpTransport::new().standalone().unwrap();
        let transport_handle = transport.handle("http://localhost:5000").unwrap();
        Mutex::new(ControllerRpc::new(transport_handle))
    };
}

macro_rules! rpc_method {
    ($name:ident($($arg_name:ident: $ty:ty),*) -> $ret_type:ty) => {
        #[no_mangle]
        pub extern "C" fn $name(
            $($arg_name: $ty),*,
            error_code: *mut i64,
            error_message: *mut *const c_char,
            error_data: *mut *const c_char
        ) -> $ret_type {
            let pass_ownership= |s: String| unsafe {
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

macro_rules! rpc_methods {
    {
        $($name:ident($($arg_name:ident: $ty:ty),*) -> $ret_type:ty)*
    } => {
        $(rpc_method!($name($(arg_name: $ty),*) -> $ret_type);)*
    }
}

rpc_methods!{
    Test(val: i32) -> i32
}