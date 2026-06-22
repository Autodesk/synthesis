/// <reference types="vite/client" />
interface ImportMetaEnv {
    // biome-ignore lint/style/useNamingConvention: environment variable
    readonly VITE_MULTIPLAYER_PORT: string
}

// biome-ignore lint/correctness/noUnusedVariables: funky env stuff it is used
interface ImportMeta {
    readonly env: ImportMetaEnv
}
