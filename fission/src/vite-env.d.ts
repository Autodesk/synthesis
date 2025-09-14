/// <reference types="vite/client" />

// Electron Forge Vite plugin environment variables
declare const MAIN_WINDOW_VITE_DEV_SERVER_URL: string | undefined
declare const MAIN_WINDOW_VITE_NAME: string

// Type declaration for electron-squirrel-startup
declare module "electron-squirrel-startup" {
    const started: boolean
    export = started
}

interface Window {
    electronAPI?: {
        platform: string
    }
}

interface ImportMetaEnv {
    // biome-ignore lint/style/useNamingConvention: environment variable
    readonly VITE_MULTIPLAYER_PORT: string
}

// biome-ignore lint/correctness/noUnusedVariables: funky env stuff it is used
interface ImportMeta {
    readonly env: ImportMetaEnv
}
