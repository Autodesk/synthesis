import { defineConfig } from 'vitest/config'
import * as path from 'path'
import react from '@vitejs/plugin-react-swc'
import basicSsl from '@vitejs/plugin-basic-ssl'
import glsl from 'vite-plugin-glsl';

const basePath = '/fission/'
const serverPort = 3000
const dockerServerPort = 80

const useLocal = false
const useSsl = true

const plugins = [
    react(), glsl({
        include: [                   // Glob pattern, or array of glob patterns to import
          '**/*.glsl', '**/*.wgsl',
          '**/*.vert', '**/*.frag',
          '**/*.vs', '**/*.fs'
        ],
        exclude: undefined,          // Glob pattern, or array of glob patterns to ignore
        warnDuplicatedImports: true, // Warn if the same chunk was imported multiple times
        defaultExtension: 'glsl',    // Shader suffix when no extension is specified
        compress: false,             // Compress output shader code
        watch: true,                 // Recompile shader on change
        root: '/'                    // Directory for root imports
    })
]

if (useSsl) {
    plugins.push(basicSsl())
}

// https://vitejs.dev/config/
export default defineConfig({
    plugins: plugins,
    resolve: {
        alias: [
            { find: '@/components', replacement: path.resolve(__dirname, 'src', 'ui', 'components') },
            { find: '@/modals', replacement: path.resolve(__dirname, 'src', 'ui', 'modals') },
            { find: '@/panels', replacement: path.resolve(__dirname, 'src', 'ui', 'panels') },
            { find: '@', replacement: path.resolve(__dirname, 'src') }
        ]
    },
    test: {
        testTimeout: 5000,
        globals: true,
        environment: 'jsdom',
        browser: {
            enabled: true,
            name: 'chromium',
            headless: true,
            provider: 'playwright'
        }
    },
    server: {
        // this ensures that the browser opens upon server start
        // open: true,
        // this sets a default port to 3000
        port: serverPort,
        cors: false,
        proxy: useLocal ? {
            '/api/mira': {
                target: `http://localhost:${serverPort}${basePath}`,
                changeOrigin: true,
                secure: false,
                rewrite: (path) => path.replace(/^\/api\/mira/, '/Downloadables/Mira')
            },
            '/api/aps': {
                target: `http://localhost:${dockerServerPort}/`,
                changeOrigin: true,
                secure: false,
            },
        } : {
            '/api': {
                target: `https://synthesis.autodesk.com/`,
                changeOrigin: true,
                secure: true,
            },
        },
    },
    build: {
        target: 'esnext',
    },
    base: basePath
})
