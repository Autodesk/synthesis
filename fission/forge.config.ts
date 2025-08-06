import { FuseV1Options, FuseVersion } from "@electron/fuses"
import { MakerSquirrel } from "@electron-forge/maker-squirrel"
import { MakerZIP } from "@electron-forge/maker-zip"
import { FusesPlugin } from "@electron-forge/plugin-fuses"
import { VitePlugin } from "@electron-forge/plugin-vite"
import type { ForgeConfig } from "@electron-forge/shared-types"
import path from "path"

const config: ForgeConfig = {
  packagerConfig: {
    asar: true,
    name: "Synthesis",
    executableName: "Synthesis",
    icon: path.resolve(__dirname, "src/assets/icons/synthesis-logo"),
  },
  rebuildConfig: {},
  makers: [
    new MakerSquirrel({
      setupIcon: path.resolve(__dirname, "src/assets/icons/synthesis-logo.ico"),
    }),
    new MakerZIP({}, ["darwin", "linux"]),
  ],
  plugins: [
    new VitePlugin({
      build: [
        {
          entry: "src/main.ts",
          config: "electron/vite.main.config.ts",
          target: "main",
        },
        {
          entry: "src/preload.ts",
          config: "electron/vite.preload.config.ts",
          target: "preload",
        },
      ],
      renderer: [
        {
          name: "main_window",
          config: "electron/vite.renderer.config.ts",
        },
      ],
    }),
    // Fuses are used to enable/disable various Electron functionality
    // at package time, before code signing the application
    new FusesPlugin({
      version: FuseVersion.V1,
      [FuseV1Options.RunAsNode]: false,
      [FuseV1Options.EnableCookieEncryption]: true,
      [FuseV1Options.EnableNodeOptionsEnvironmentVariable]: false,
      [FuseV1Options.EnableNodeCliInspectArguments]: false,
      [FuseV1Options.EnableEmbeddedAsarIntegrityValidation]: true,
      [FuseV1Options.OnlyLoadAppFromAsar]: true,
    }),
  ],
}

export default config
