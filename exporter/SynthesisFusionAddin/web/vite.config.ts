import { defineConfig } from "vite"
import react from "@vitejs/plugin-react-swc"

// https://vite.dev/config/
export default defineConfig({
    plugins: [react()],
    base: "./",
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    mui: ["@mui/material", "@mui/icons-material"]
                },
            },
        },
    },
})
