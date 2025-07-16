import { defineConfig, loadEnv } from "vite"
import react from "@vitejs/plugin-react-swc"

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), "")
    return {
        plugins: [react()],
        base: "./",
        build: {
            target: "esnext",
            rollupOptions: {
                output: {
                    manualChunks: {
                        mui: ["@mui/material", "@mui/icons-material"],
                    },
                },
            },
            sourcemap: env.NODE_ENV == "dev" ? "inline": false,
        },
    }
})
