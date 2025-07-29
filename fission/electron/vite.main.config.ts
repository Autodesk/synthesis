import { defineConfig } from "vite"

// https://vitejs.dev/config
export default defineConfig({
  build: {
    lib: {
      entry: "src/main.ts",
      formats: ["cjs"],
      fileName: () => "main.cjs",
    },
    rollupOptions: {
      external: ["electron"],
    },
  },
  resolve: {
    // Some libs that can run in both Web and Node.js, we need to tell Vite to build them in Node.js.
    conditions: ["node"],
    mainFields: ["module", "jsnext:main", "jsnext"],
  },
})
