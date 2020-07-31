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

// TODO create a macro to make these functions for us
#[no_mangle]
pub extern "C" fn Test(val: i32, error_code: *mut i64, error_message: *mut *const c_char, error_data: *mut *const c_char) -> i32 {
    match CLIENT.lock().unwrap().Test(val).call() {
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
                    unsafe { *error_message = json_rpc_error.message.as_bytes().as_ptr() as *const i8; } // TODO null-terminate strings
                }
                if error_data != null_mut() {
                    match &json_rpc_error.data {
                        Some(value) => match value {
                            Value::String(data_str) => unsafe { *error_data = data_str.as_bytes().as_ptr() as *const i8 },
                            _ => ()
                        },
                        None => ()
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
    return -1;
}