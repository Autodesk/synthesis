import { defineConfig } from 'vitest/config'
import * as path from 'path'
import react from '@vitejs/plugin-react-swc'
import glsl from 'vite-plugin-glsl';

const basePath = '/fission/'
const serverPort = 3000
const dockerServerPort = 3003

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), /* viteSingleFile(), */ glsl({
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
  }) ],
  resolve: {
      alias: [
          { find: '@', replacement: path.resolve(__dirname, 'src') }
      ]
  },
  test: {
    globals: true,
    environment: 'jsdom'
  },
  server: {
    // this ensures that the browser opens upon server start
    open: true,
    // this sets a default port to 3000
    port: serverPort,
    cors: false,
    proxy: {
      '/api/mira': {
        target: `http://localhost:${serverPort}${basePath}`,
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api\/mira/, '/Downloadables/Mira')
      },
      '/api/auth': {
        target: `http://localhost:${dockerServerPort}/`,
        changeOrigin: true,
        secure: false
      }
    },
  },
  build: {
    target: 'esnext',
  },
  base: basePath
})
