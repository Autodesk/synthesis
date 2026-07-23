# https://nixos.org/download/
# https://wiki.nixos.org/wiki/Flakes#Setup
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
        f:
        nixpkgs.lib.genAttrs supportedSystems (
          system:
          f {
            inherit system;
            pkgs = nixpkgs.legacyPackages.${system};
          }
        );
    in
    {
      devShells = forEachSupportedSystem (
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
          multiplayer = pkgs.mkShell {
            packages = with pkgs; [
              bun
            ];
          };
        }
      );

      formatter = forEachSupportedSystem ({ pkgs, ... }: pkgs.nixfmt-tree);

      # Build all devShells, instead of just verifying they are derivations
      checks = forEachSupportedSystem ({ system, ... }: self.devShells.${system});
    };
}
