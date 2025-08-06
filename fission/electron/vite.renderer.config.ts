import { defineConfig, mergeConfig } from "vite"
import baseConfig from "../vite.config"

// https://vitejs.dev/config
export default defineConfig((env) => mergeConfig(baseConfig(env), {}))
