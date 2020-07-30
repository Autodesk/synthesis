#[macro_use] extern crate jsonrpc_client_core;

#[macro_use] extern crate jsonrpc_client_http;


use jsonrpc_client_http::HttpTransport;

jsonrpc_client!(pub struct ControllerRpc {
    pub fn Forward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Backward(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Left(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
    pub fn Right(&mut self, channel: u32, distance: f64) -> RpcRequest<()>;
});

