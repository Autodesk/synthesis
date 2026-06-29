/// <reference types="vite/client" />
interface ImportMetaEnv {
    // biome-ignore lint/style/useNamingConvention: environment variable
    readonly VITE_MULTIPLAYER_PORT: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
