# https://nixos.org/download/
# https://wiki.nixos.org/wiki/Flakes#Setup
{
  description = "Synthesis' Web-Based Robotics Simulator";

  nixConfig = {
    commit-lock-file-summary = "chore: update flake.lock";
  };

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    systems.url = "github:nix-systems/triplet";
  };

  outputs =
    {
      self,
      nixpkgs,
      systems,
    }:
    let
      inherit (nixpkgs) lib;

      forEachSystem =
        f:
        lib.genAttrs (import systems) (
          system:
          f {
            inherit system;
            pkgs = nixpkgs.legacyPackages.${system};
          }
        );
    in
    {
      devShells = forEachSystem (
        { pkgs, system }:
        {
          default = self.devShells.${system}.fission;
          fission = pkgs.mkShell {
            packages = with pkgs; [
              nodejs
              bun
              git-lfs
              playwright-test
              playwright-driver.browsers
            ];

            env = {
              PLAYWRIGHT_BROWSERS_PATH = pkgs.playwright-driver.browsers;
              PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS = "true";
              PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD = "1";
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
          glueball = pkgs.mkShell {
            packages = with pkgs; [
              cargo
              clippy
              rustfmt
              rust-analyzer
            ];
          };
        }
      );

      formatter = forEachSystem ({ pkgs, ... }: pkgs.nixfmt-tree);

      # Build all devShells, instead of just verifying they are derivations
      checks = forEachSystem ({ system, ... }: self.devShells.${system});
    };
}
