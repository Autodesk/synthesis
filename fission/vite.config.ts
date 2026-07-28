import fs from "node:fs/promises"
import basicSsl from "@vitejs/plugin-basic-ssl"
import react from "@vitejs/plugin-react-swc"
import * as path from "path"

import { loadEnv } from "vite"
import glsl from "vite-plugin-glsl"

import { defineConfig, type ViteUserConfig } from "vitest/config"

const serverPort = 3000
const dockerServerPort = 80

const useLocalAPS = false
const useSsl = false

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
    const useLocalAssets = localAssetsExist && (mode === "test" || process.env.NODE_ENV == "development")

    if (!localAssetsExist && (mode === "test" || process.env.NODE_ENV == "development")) {
        console.warn("Can't find local assets, do you need to run `npm run assetpack`?")
    }
    console.log(`Using ${useLocalAssets ? "local" : "remote"} mirabuf assets`)

    const proxies: Proxies = {}
    const assetProxy: ProxyOptions = useLocalAssets
        ? {
              target: `http://localhost:${mode === "test" ? 3001 : serverPort}`,
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
    proxies["/api/aps"] = useLocalAPS
        ? {
              target: `http://localhost:${dockerServerPort}/`,
              changeOrigin: true,
              secure: false,
          }
        : {
              target: `https://synthesis.autodesk.com/`,
              changeOrigin: true,
              secure: true,
          }
    return {
        plugins: plugins as ViteUserConfig["plugins"],
        publicDir: "./public",
        resolve: {
            alias: [
                { find: "@/components", replacement: path.resolve(__dirname, "src", "ui", "components") },
                { find: "@/modals", replacement: path.resolve(__dirname, "src", "ui", "modals") },
                { find: "@/panels", replacement: path.resolve(__dirname, "src", "ui", "panels") },
                { find: "@", replacement: path.resolve(__dirname, "src") },
            ],
        },
        define: {
            GIT_COMMIT: JSON.stringify(commitHash),
        },
        test: {
            setupFiles: ["src/test/TestSetup.browser.ts"],
            globalSetup: ["src/test/TestSetup.server.ts"],
            testTimeout: 10000,
            globals: true,
            environment: "jsdom",
            reporters: process.env.GITHUB_ACTIONS
                ? [
                      "github-actions",
                      "default",
                      {
                          onTestRunEnd(_testModules, unhandledErrors, reason) {
                              if (reason === "passed" && unhandledErrors.length === 0) {
                                  console.error("GH ACTIONS VITEST PASSED")
                              } else {
                                  console.error(unhandledErrors)
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
            coverage: {
                provider: "istanbul",
                reporter: ["text", "html"] as const,
                reportsDirectory: "./coverage",
                include: ["src/**/*.{ts,tsx}"],
                exclude: ["src/test/**", "src/proto/**"],
                reportOnFailure: true,
            },
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
