extern crate controller_sys;

use controller_sys::*;
use jsonrpc_client_http::HttpTransport;
use std::thread::sleep;
use std::time::Duration;

fn main() {
    let transport = HttpTransport::new().standalone().unwrap();
    let transport_handle = transport.handle("http://localhost:5000").unwrap();
    let mut client = ControllerRpc::new(transport_handle);
    println!("forward");
    let _ = client.Forward(5, 10.0);
    sleep(Duration::from_secs(3));
    println!("left");
    let _ = client.Left(5, 10.0);
    sleep(Duration::from_secs(3));
    println!("back");
    let _ = client.Backward(5, 10.0);
    sleep(Duration::from_secs(3));
    println!("right");
    let _ = client.Right(5, 10.0);
    sleep(Duration::from_secs(3));
}
