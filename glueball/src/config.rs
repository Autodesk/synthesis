use std::{
    fs::read_to_string,
    path::{Path, PathBuf},
};

use argh::FromArgs;
use directories::ProjectDirs;
use toml::{Table, Value};

use crate::util::tilde_expansion;

pub const DEFAULT_PORT: u32 = 2610;

pub fn certification_directory() -> PathBuf {
    ProjectDirs::from("com", "Autodesk", "synthesis-glueball")
        .expect("Could not find certificate directory")
        .data_dir()
        .join("secrets")
}

#[derive(FromArgs)]
#[argh(description = "A Server for Facilitating Multiplayer Synthesis")]
pub struct CliConfig {
    #[argh(
        option,
        description = "path to configuration file. additional command-line flags will override options set in the config file"
    )]
    pub config_file: Option<PathBuf>,

    #[argh(
        option,
        description = "directory in which to store certificates and key files (only for --secure mode)"
    )]
    pub cert_dir: Option<PathBuf>,

    // This doesn't have a default because otherwise we wouldn't be able to have it take precedence
    // over the file config analog of this argument properly
    #[argh(option, short = 'p', description = "port to listen on")]
    pub port: Option<u32>,

    #[argh(
        switch,
        short = 'h',
        description = "show a text console instead of the interactive TUI"
    )]
    pub headless: bool,

    #[argh(
        switch,
        short = 's',
        description = "encrypt websocket traffic using TLS. self-signed PEM certificates will be automatically generated"
    )]
    pub secure: bool,

    #[argh(
        option,
        short = 'r',
        description = "create a persistent room with this code that is always available. code must be a six-character string consisting only of valid base-10 digits and uppercase characters"
    )]
    pub permanent_room: Option<String>,
}

pub fn parse_config_file<P>(path: P, old_config: &mut CliConfig)
where
    P: AsRef<Path>,
{
    let config = read_to_string(path).expect("Error: config file not found ");
    let table = config
        .parse::<Table>()
        .expect("Error: config file not valid toml");
    old_config.config_file = None;

    if let Some(Value::String(path)) = &table.get("cert-dir")
        && old_config.cert_dir.is_none()
    {
        let path = PathBuf::from(path);
        old_config.cert_dir = Some(path);
    }

    if let Some(Value::Integer(port)) = &table.get("port")
        && old_config.port.is_none()
    {
        old_config.port = u32::try_from(*port).ok();
    }

    if let Some(Value::Boolean(headless)) = &table.get("headless")
        && !old_config.headless
    {
        old_config.headless = *headless;
    }

    if let Some(Value::Boolean(secure)) = &table.get("secure")
        && !old_config.secure
    {
        old_config.secure = *secure;
    }

    if let Some(Value::String(room_id)) = &table.get("permanent_room")
        && old_config.permanent_room.is_none()
    {
        old_config.permanent_room = Some(room_id.clone());
    }
}

pub fn config_or_default(config: &CliConfig) -> (PathBuf, u32) {
    let mut cert_dir = config
        .cert_dir
        .clone()
        .unwrap_or_else(certification_directory);
    let port = config.port.unwrap_or(DEFAULT_PORT);

    tilde_expansion(&mut cert_dir);

    (cert_dir, port)
}
