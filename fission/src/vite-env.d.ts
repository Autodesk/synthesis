/// <reference types="vite/client" />
interface ImportMetaEnv {
    // biome-ignore lint/style/useNamingConvention: environment variable
    readonly VITE_MULTIPLAYER_PORT: string
    // biome-ignore lint/style/useNamingConvention: environment variable
    readonly VITE_JOLT_LEAK_CHECK?: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
