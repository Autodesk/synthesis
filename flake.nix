{
  description = "Synthesis' Web-Based Robotics Simulator";

  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";

  outputs =
    { self, nixpkgs }:
    let
      supportedSystems = [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
      forEachSupportedSystem =
        f: nixpkgs.lib.genAttrs supportedSystems (system: f nixpkgs.legacyPackages.${system});
    in
    {
      devShells = forEachSupportedSystem (pkgs: {
        default = self.devShells.${pkgs.stdenv.hostPlatform.system}.fission;
        fission = pkgs.mkShell {
          nativeBuildInputs = with pkgs; [
            nodejs
            bun
            git-lfs
            playwright-driver.browsers
          ];

          env = {
            PLAYWRIGHT_BROWSERS_PATH = pkgs.playwright-driver.browsers;
            PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS = true;
          };
        };
        exporter = pkgs.mkShell {
          packages = with pkgs; [
            python3
            black
            isort
            bun
          ];
        };
      });

      formatter = forEachSupportedSystem (pkgs: pkgs.nixfmt-tree);

      # Build all devShells, instead of just verifying they are deviations
      checks = forEachSupportedSystem (pkgs: self.devShells.${pkgs.stdenv.hostPlatform.system});
    };
}
