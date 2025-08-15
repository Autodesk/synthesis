import { defineConfig, mergeConfig, type UserConfig } from "vite"
import baseConfig from "../vite.config"

// https://vitejs.dev/config
export default defineConfig(async (env) => {
    const base = typeof baseConfig === 'function' ? await baseConfig(env) : baseConfig
    return mergeConfig(base, {} as UserConfig)
})
