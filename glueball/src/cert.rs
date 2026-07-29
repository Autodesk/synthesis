use std::{
    error::Error,
    fs::{self, File},
    io::{self, BufReader, ErrorKind},
    path::PathBuf,
};

use directories::ProjectDirs;
use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio_rustls::rustls::{
    ServerConfig,
    pki_types::{CertificateDer, PrivateKeyDer},
};

pub fn get_cert_directory() -> Option<PathBuf> {
    ProjectDirs::from("com", "synthesis", "glueball").map(|dirs| dirs.config_dir().join("keys"))
}

/// Creates a TLS config for the server
/// Generates a certificate if one does not exist
pub fn build_tls_config() -> Result<ServerConfig, Box<dyn Error>> {
    let cert_dir = get_cert_directory()
        .ok_or_else(|| io::Error::new(ErrorKind::NotFound, "Could not determine home directory"))?;
    ensure_certificate(&cert_dir)?;

    let mut cert_reader = BufReader::new(File::open(cert_dir.join("cert.pem"))?);
    let cert_chain: Vec<CertificateDer> =
        rustls_pemfile::certs(&mut cert_reader).collect::<Result<_, _>>()?;

    let mut key_reader = BufReader::new(File::open(cert_dir.join("key.pem"))?);
    let key = rustls_pemfile::pkcs8_private_keys(&mut key_reader)
        .next()
        .ok_or_else(|| {
            format!(
                "Invalid PKCS#8 private key found in {}/key.pem",
                cert_dir.to_str().unwrap()
            )
        })??;

    let config = ServerConfig::builder()
        .with_no_client_auth()
        .with_single_cert(cert_chain, PrivateKeyDer::Pkcs8(key))?;

    Ok(config)
}

/// Writes a self-signed certificate and keypair to `cert_dir` if one isn't already present.
fn ensure_certificate(cert_dir: &PathBuf) -> Result<(), Box<dyn Error>> {
    if !fs::exists(cert_dir)? {
        fs::create_dir_all(cert_dir)?;
    }

    if fs::exists(cert_dir.join("cert.pem"))? {
        return Ok(());
    }

    let subject_alt_names = vec!["localhost".to_string(), "127.0.0.1".to_string()];
    let CertifiedKey { cert, signing_key } = generate_simple_self_signed(subject_alt_names)?;

    fs::write(cert_dir.join("cert.pem"), cert.pem())?;
    fs::write(cert_dir.join("key.pem"), signing_key.serialize_pem())?;

    Ok(())
}
