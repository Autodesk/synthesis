use std::{
    fs::read_to_string,
    path::{Path, PathBuf},
    time::Duration,
};

use anyhow::{Result, bail};
use argh::FromArgs;
use directories::ProjectDirs;
use toml::{Table, Value};
use wtransport::{Identity, ServerConfig};

use crate::util::tilde_expansion;

pub const DEFAULT_PORT: u16 = 2610;

/// How often to poke an otherwise idle connection so QUIC doesn't time it out.
const KEEP_ALIVE_INTERVAL: Duration = Duration::from_secs(10);

pub fn certification_directory() -> Result<PathBuf> {
    let dir = match ProjectDirs::from("com", "Autodesk", "synthesis-glueball") {
        Some(dirs) => dirs.data_dir().join("secrets"),
        None => bail!("Cound not find certificate directory"),
    };

    Ok(dir)
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
        description = "directory in which to store certificates and key files"
    )]
    pub cert_dir: Option<PathBuf>,

    // This doesn't have a default because otherwise we wouldn't be able to have it take precedence
    // over the file config analog of this argument properly
    #[argh(option, short = 'p', description = "port to listen on")]
    pub port: Option<u16>,

    #[argh(
        switch,
        short = 'h',
        description = "show a text console instead of the interactive TUI"
    )]
    pub headless: bool,

    #[argh(
        option,
        short = 'r',
        description = "create a persistent room with this code that is always available. code must be a six-character string consisting only of valid base-10 digits and uppercase characters"
    )]
    pub permanent_room: Option<String>,
}

pub fn parse_config_file<P>(path: P, old_config: &mut CliConfig) -> Result<()>
where
    P: AsRef<Path>,
{
    let Ok(config) = read_to_string(&path) else {
        bail!("Config file not found");
    };

    let Ok(table) = config.parse::<Table>() else {
        bail!("Config file not valid TOML")
    };
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
        old_config.port = u16::try_from(*port).ok();
    }

    if let Some(Value::Boolean(headless)) = &table.get("headless")
        && !old_config.headless
    {
        old_config.headless = *headless;
    }

    if let Some(Value::String(room_id)) = &table.get("permanent_room")
        && old_config.permanent_room.is_none()
    {
        old_config.permanent_room = Some(room_id.clone());
    }

    Ok(())
}

pub struct AppConfig {
    pub cert_dir: PathBuf,
    pub permanent_room: Option<String>,
    pub port: u16,
    pub headless: bool,
}

fn config_or_default(config: CliConfig) -> Result<AppConfig> {
    let mut cert_dir = config
        .cert_dir
        .clone()
        .unwrap_or(certification_directory()?);
    let port = config.port.unwrap_or(DEFAULT_PORT);

    tilde_expansion(&mut cert_dir)?;

    Ok(AppConfig {
        cert_dir,
        permanent_room: config.permanent_room,
        port,
        headless: config.headless,
    })
}

pub fn retrieve_config() -> Result<AppConfig> {
    let mut config: CliConfig = argh::from_env();

    if let Some(config_file) = config.config_file.clone() {
        parse_config_file(config_file, &mut config)?;
    }

    config_or_default(config)
}

pub fn build_server_config(port: u16, identity: Identity) -> ServerConfig {
    ServerConfig::builder()
        .with_bind_default(port)
        .with_identity(identity)
        .keep_alive_interval(Some(KEEP_ALIVE_INTERVAL))
        .build()
}
