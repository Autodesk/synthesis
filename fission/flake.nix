{
  description = "Synthesis' Web-Based Robotics Simulator";

  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";

  outputs =
    inputs:
    let
      supportedSystems = [
        "x86_64-linux"
        "aarch64-linux"
        "x86_64-darwin"
        "aarch64-darwin"
      ];
      forEachSupportedSystem =
        f: inputs.nixpkgs.lib.genAttrs supportedSystems (system: f inputs.nixpkgs.legacyPackages.${system});
    in
    {
      devShells = forEachSupportedSystem (pkgs: {
        default = pkgs.mkShell {
          nativeBuildInputs = with pkgs; [
            playwright-driver.browsers
          ];

          env = {
            PLAYWRIGHT_BROWSERS_PATH = pkgs.playwright-driver.browsers;
            PLAYWRIGHT_SKIP_VALIDATE_HOST_REQUIREMENTS = true;
          };
        };
      });

      formatter = forEachSupportedSystem (pkgs: pkgs.nixfmt-tree);
    };
}
