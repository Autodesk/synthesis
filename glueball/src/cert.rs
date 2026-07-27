use std::{
    error::Error,
    fs::{self, File},
    io::BufReader,
};

use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio_rustls::rustls::{
    ServerConfig,
    pki_types::{CertificateDer, PrivateKeyDer},
};

/// Creates a TLS config for the server
/// Generates a certificate if one does not exist
pub fn build_tls_config() -> Result<ServerConfig, Box<dyn Error>> {
    ensure_certificate()?;

    let mut cert_reader = BufReader::new(File::open("./secrets/cert.pem")?);
    let cert_chain: Vec<CertificateDer> =
        rustls_pemfile::certs(&mut cert_reader).collect::<Result<_, _>>()?;

    let mut key_reader = BufReader::new(File::open("./secrets/key.pem")?);
    let key = rustls_pemfile::pkcs8_private_keys(&mut key_reader)
        .next()
        .ok_or("Invalid PKCS#8 private key found in ./secrets/key.pem")??;

    let config = ServerConfig::builder()
        .with_no_client_auth()
        .with_single_cert(cert_chain, PrivateKeyDer::Pkcs8(key))?;

    Ok(config)
}

/// Writes a self-signed certificate and keypair to `./secrets` if one isn't already present.
fn ensure_certificate() -> Result<(), Box<dyn Error>> {
    if !fs::exists("./secrets")? {
        fs::create_dir("./secrets")?;
    }

    if fs::exists("./secrets/cert.pem")? {
        return Ok(());
    }

    let subject_alt_names = vec!["localhost".to_string(), "127.0.0.1".to_string()];
    let CertifiedKey { cert, signing_key } = generate_simple_self_signed(subject_alt_names)?;

    fs::write("./secrets/cert.pem", cert.pem())?;
    fs::write("./secrets/key.pem", signing_key.serialize_pem())?;

    Ok(())
}
