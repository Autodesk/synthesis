{
  lib,
  rustPlatform,
}:
rustPlatform.buildRustPackage {
  pname = "glueball";
  inherit ((lib.importTOML ./Cargo.toml).package) version;

  src = ./.;

  cargoLock.lockFile = ./Cargo.lock;

  meta = {
    description = "Proxy server for facilitating multiplayer interactions in Autodesk Synthesis";
    homepage = "https://synthesis.autodesk.com";
    mainProgram = "glueball";
    license = lib.licenses.asl20;
  };
}
