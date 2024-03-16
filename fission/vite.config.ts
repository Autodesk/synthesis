import { defineConfig } from 'vite'
import * as path from 'path'
import react from '@vitejs/plugin-react-swc'
import { viteSingleFile } from 'vite-plugin-singlefile'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), /* viteSingleFile() */],
resolve: {
    alias: [
        { find: '@', replacement: path.resolve(__dirname, 'src') }
    ]
},
  server: {    
    // this ensures that the browser opens upon server start
    open: true,
    // this sets a default port to 3000  
    port: 3000, 
  }
})
