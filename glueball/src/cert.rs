use std::{
    fs::{self, File},
    io::BufReader,
    path::PathBuf,
};

use anyhow::{Result, bail};
use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio_rustls::rustls::{
    ServerConfig,
    pki_types::{CertificateDer, PrivateKeyDer},
};

/// Creates a TLS config for the server
/// Generates a certificate if one does not exist
pub fn build_tls_config(cert_directory: &PathBuf) -> Result<ServerConfig> {
    ensure_certificate(cert_directory)?;

    let mut cert_reader = BufReader::new(File::open(cert_directory.join("cert.pem"))?);
    let cert_chain: Vec<CertificateDer> =
        rustls_pemfile::certs(&mut cert_reader).collect::<Result<_, _>>()?;

    let key_path = cert_directory.join("key.pem");
    let mut key_reader = BufReader::new(File::open(&key_path)?);

    let Some(Ok(key)) = rustls_pemfile::pkcs8_private_keys(&mut key_reader).next() else {
        let Some(path) = key_path.to_str() else {
            bail!("Certificate file must be unicode");
        };

        bail!("Invalid PKCS#8 private key found in {path}");
    };

    let config = ServerConfig::builder()
        .with_no_client_auth()
        .with_single_cert(cert_chain, PrivateKeyDer::Pkcs8(key))?;

    Ok(config)
}

/// Writes a self-signed certificate and keypair to `path` if one isn't already present.
fn ensure_certificate(path: &PathBuf) -> Result<()> {
    if !fs::exists(path)? {
        fs::create_dir_all(path)?;
    }

    if fs::exists(path.join("cert.pem"))? {
        return Ok(());
    }

    let subject_alt_names = vec!["localhost".to_string(), "127.0.0.1".to_string()];
    let CertifiedKey { cert, signing_key } = generate_simple_self_signed(subject_alt_names)?;

    fs::write(path.join("cert.pem"), cert.pem())?;
    fs::write(path.join("key.pem"), signing_key.serialize_pem())?;

    Ok(())
}
