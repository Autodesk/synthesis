import fs from "node:fs/promises"
import basicSsl from "@vitejs/plugin-basic-ssl"
import react from "@vitejs/plugin-react-swc"
import * as path from "path"

import { loadEnv } from "vite"
import glsl from "vite-plugin-glsl"

import {
    defineConfig,
    type TestProjectConfiguration,
    type TestProjectInlineConfiguration,
    type ViteUserConfig,
} from "vitest/config"

/**
 * SYNTHESIS PARAMETERS
 */

// Developers can override any of these with SYNTHESIS_* variables in a local, gitignored `.env` file
// (see `.env.template`). Variables set in the shell take precedence over the file.
const localEnv = loadEnv("", process.cwd(), "SYNTHESIS_")

const serverPort = Number(localEnv.SYNTHESIS_SERVER_PORT ?? 3000)

const useSsl = ["true", "1"].includes(localEnv.SYNTHESIS_USE_SSL ?? "false")

// APS configuration. These default to production; internal developers can override them in `.env`
// to test against a locally running Synthesis API.
const apsClientId = localEnv.SYNTHESIS_APS_CLIENT_ID ?? "GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU"
const apsCallbackUrl = localEnv.SYNTHESIS_APS_CALLBACK_URL ?? "https://synthesis.autodesk.com/fission/"
const apsAuthenticationBaseUrl = localEnv.SYNTHESIS_APS_AUTHENTICATION_BASE_URL ?? "https://developer.api.autodesk.com"
const apsDataManagementBaseUrl = localEnv.SYNTHESIS_APS_DATA_MANAGEMENT_BASE_URL ?? "https://developer.api.autodesk.com"
const apsUserinfoBaseUrl = localEnv.SYNTHESIS_APS_USERINFO_BASE_URL ?? "https://api.aps.autodesk.com"

const plugins = [
    react(),
    glsl({
        include: [
            // Glob pattern, or array of glob patterns to import
            "**/*.glsl",
            "**/*.wgsl",
            "**/*.vert",
            "**/*.frag",
            "**/*.vs",
            "**/*.fs",
        ],
        exclude: undefined, // Glob pattern, or array of glob patterns to ignore
        warnDuplicatedImports: true, // Warn if the same chunk was imported multiple times
        defaultExtension: "glsl", // Shader suffix when no extension is specified
        minify: false, // Minify/optimize output shader code
        watch: true, // Recompile shader on change
        root: "/", // Directory for root imports
    }),
]

if (useSsl) {
    plugins.push(basicSsl())
}

const localAssetsExist = await fs
    .access("./public/Downloadables/mira", fs.constants.R_OK)
    .then(() => true)
    .catch(() => false)

const commitHash = await getCommitHash()

type Proxies = Required<Required<ViteUserConfig>["server"]>["proxy"]
type ProxyOptions = Proxies[string]

// https://vitejs.dev/config/
export default defineConfig(({ mode }): ViteUserConfig => {
    process.env = { ...process.env, ...loadEnv(mode, process.cwd()) }
    process.env.VITE_MULTIPLAYER_PORT = mode === "test" ? "3001" : "9002"
    // @vitest/browser spawns its vite server with mode "test" for `vitest test` and
    // "benchmark" for `vitest bench`; both should use local assets when available
    // (private mirabuf assets such as Multi-Joint Wheels only exist locally).
    const useLocalAssets =
        localAssetsExist && (mode === "test" || mode === "benchmark" || process.env.NODE_ENV == "development")

    if (!localAssetsExist && mode !== "production") {
        console.warn("Can't find local assets, do you need to run `npm run assetpack`?")
    }
    console.log(`Using ${useLocalAssets ? "local" : "remote"} mirabuf assets`)

    const proxies: Proxies = {}
    // In dev mode NODE_ENV is "development"; in vitest (test or bench) it is "test"
    // regardless of what mode @vitest/browser uses when spawning the browser vite server.
    const localAssetPort = process.env.NODE_ENV === "development" ? serverPort : 3001
    const assetProxy: ProxyOptions = useLocalAssets
        ? {
              target: `http://localhost:${localAssetPort}`,
              changeOrigin: true,
              secure: false,
              rewrite: path => path.replace(/^\/api/, "/Downloadables"),
          }
        : {
              target: `https://synthesis.autodesk.com/`,
              changeOrigin: true,
              secure: true,
          }
    proxies["/api/mira"] = assetProxy
    proxies["/api/match_configs"] = assetProxy
    const baseAliases = [
        { find: "@/components", replacement: path.resolve(__dirname, "src", "ui", "components") },
        { find: "@/modals", replacement: path.resolve(__dirname, "src", "ui", "modals") },
        { find: "@/panels", replacement: path.resolve(__dirname, "src", "ui", "panels") },
        { find: "@", replacement: path.resolve(__dirname, "src") },
    ]

    const fissionProject: TestProjectInlineConfiguration = {
        extends: true,
        test: {
            name: "fission",
            setupFiles: ["src/test/TestSetup.browser.ts"],
            globalSetup: ["src/test/TestSetup.server.ts"],
            testTimeout: 10000,
            globals: true,
            environment: "node",
            reporters: process.env.GITHUB_ACTIONS
                ? [
                      "github-actions",
                      "default",
                      {
                          onTestRunEnd(_modules: unknown, unhandled: unknown[], reason: TestRunEndReason) {
                              if (reason === "passed" && unhandled.length === 0) {
                                  console.error("GH ACTIONS VITEST PASSED")
                              } else {
                                  console.error(unhandled)
                              }
                          },
                      },
                  ]
                : ["default"],
            browser: {
                enabled: true,
                provider: "playwright",
                instances: [
                    {
                        name: "chromium",
                        browser: "chromium",
                        headless: true,
                    },
                    {
                        name: "firefox",
                        browser: "firefox",
                        headless: true,
                    },
                ],
            },
        },
    }

    // `bun run test:asan`
    const fissionAsanProject = {
        extends: true,
        resolve: {
            alias: [
                ...baseAliases,
                {
                    find: /^@synthesis\.adsk\/jolt-physics(\/wasm-compat)?$/,
                    replacement: process.env.JOLT_ASAN_DIST,
                },
            ],
        },
        test: {
            name: "fission-asan",
            setupFiles: ["src/test/TestSetup.browser.ts"],
            globalSetup: ["src/test/TestSetup.server.ts"],
            testTimeout: 10000,
            globals: true,
            environment: "jsdom",
            browser: {
                enabled: true,
                provider: "playwright",
                instances: [
                    {
                        name: "chromium",
                        browser: "chromium",
                        headless: true,
                    },
                ],
            },
        },
    }

    return {
        plugins: plugins as ViteUserConfig["plugins"],
        publicDir: "./public",
        resolve: {
            alias: baseAliases,
        },
        define: {
            GIT_COMMIT: JSON.stringify(commitHash),
            APS_CLIENT_ID: JSON.stringify(apsClientId),
            APS_CALLBACK_URL: JSON.stringify(apsCallbackUrl),
            APS_AUTHENTICATION_BASE_URL: JSON.stringify(apsAuthenticationBaseUrl),
            APS_DATA_MANAGEMENT_BASE_URL: JSON.stringify(apsDataManagementBaseUrl),
            APS_USERINFO_BASE_URL: JSON.stringify(apsUserinfoBaseUrl),
        },
        // Pre-bundle every react-icons subpath the app imports. Listing
        // them here bundles them up front so no reload happens once tests
        // start.
        optimizeDeps: {
            include: [
                "react-icons/ai",
                "react-icons/bi",
                "react-icons/bs",
                "react-icons/fa",
                "react-icons/fa6",
                "react-icons/gi",
                "react-icons/gr",
                "react-icons/hi",
                "react-icons/io",
                "react-icons/io5",
                "react-icons/md",
            ],
        },
        test: {
            reporters: process.env.GITHUB_ACTIONS
                ? [
                      "github-actions",
                      "default",
                      {
                          onTestRunEnd(_modules, unhandled, reason) {
                              if (reason === "passed" && unhandled.length === 0) {
                                  console.error("GH ACTIONS VITEST PASSED")
                              } else {
                                  console.error(unhandled)
                              }
                          },
                      },
                  ]
                : ["default"],
            coverage: {
                provider: "istanbul",
                reporter: ["text", "html"] as const,
                reportsDirectory: "./coverage",
                include: ["src/**/*.{ts,tsx}"],
                exclude: ["src/test/**", "src/proto/**"],
                reportOnFailure: true,
            },
            projects: [
                fissionProject,
                ...(process.env.JOLT_ASAN_DIST ? [fissionAsanProject as TestProjectConfiguration] : []),
            ],
        },
        build: {
            target: "esnext",
        },
        server: {
            port: serverPort,
            cors: false,
            proxy: proxies,
        },
    }
})

async function getCommitHash() {
    try {
        const rev = (await fs.readFile("../.git/HEAD")).toString().trim()
        if (rev.indexOf(":") === -1) {
            return rev
        } else {
            return (await fs.readFile("../.git/" + rev.substring(5))).toString().trim()
        }
    } catch (e) {
        console.warn("Could not get git hash", e)
        return "unknown"
    }
}
