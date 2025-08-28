import fs from "node:fs/promises"
import basicSsl from "@vitejs/plugin-basic-ssl"
import react from "@vitejs/plugin-react-swc"
import * as path from "path"
import {loadEnv, type ProxyOptions} from "vite"
import glsl from "vite-plugin-glsl"
import {defineConfig} from "vitest/config"
import type {TestCase, TestSuite} from "vitest/node";

const basePath = "/fission/"
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

// https://vitejs.dev/config/
export default defineConfig(async ({mode}) => {
    process.env = {...process.env, ...loadEnv(mode, process.cwd())}
    process.env.VITE_MULTIPLAYER_PORT = mode === "test" ? "3001" : "9002"
    const useLocalAssets = localAssetsExist && (mode === "test" || process.env.NODE_ENV == "development")

    if (!localAssetsExist && (mode === "test" || process.env.NODE_ENV == "development")) {
        console.warn("Can't find local assets, do you need to run `npm run assetpack`?")
    }
    console.log(`Using ${useLocalAssets ? "local" : "remote"} mirabuf assets`)

    const proxies: Record<string, ProxyOptions> = {}
    const assetProxy: ProxyOptions = useLocalAssets
        ? {
            target: `http://localhost:${mode === "test" ? 3001 : serverPort}`,
            changeOrigin: true,
            secure: false,
            rewrite: path =>
                path
                    .replace(/^\/api/, "/Downloadables")
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
        plugins: plugins,
        publicDir: "./public",
        resolve: {
            alias: [
                {find: "@/components", replacement: path.resolve(__dirname, "src", "ui", "components")},
                {find: "@/modals", replacement: path.resolve(__dirname, "src", "ui", "modals")},
                {find: "@/panels", replacement: path.resolve(__dirname, "src", "ui", "panels")},
                {find: "@", replacement: path.resolve(__dirname, "src")},
            ],
        },
        define: {
            GIT_COMMIT: JSON.stringify(await getCommitHash()),
        },
        test: {
            setupFiles: ["src/test/TestSetup.browser.ts"],
            globalSetup: ["src/test/TestSetup.server.ts"],
            testTimeout: 10000,
            globals: true,
            environment: "jsdom",
            reporters: (process.env.GITHUB_ACTIONS
                ? ["github-actions", "default", {
                    onTestCaseResult(test:TestCase) {
                        if (!test.ok()) {
                            console.warn(test.fullName, "failed")
                        }
                    },
                    onTestSuiteResult(testSuite: TestSuite) {
                        const ok = testSuite.ok()

                        if (!ok) setTimeout(() => process.exit(1), 1000)
                    }
                }] : ["default"])
            ,
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
        build: {
            target: "esnext",
        },
        server: {
            // this ensures that the browser opens upon server start
            // open: true,
            // this sets a default port to 3000
            port: serverPort,
            cors: false,
            proxy: proxies,
            build: {
                target: "esnext",
            },
            base: basePath,
        },
    }
})


async function getCommitHash() {
    try {
        const rev = (await fs.readFile('../.git/HEAD')).toString().trim();
        if (rev.indexOf(':') === -1) {
            return rev;
        } else {
            return (await fs.readFile('../.git/' + rev.substring(5))).toString().trim();
        }
    } catch (e) {
        console.warn("Could not get git hash", e)
        return "unknown"
    }
}