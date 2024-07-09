/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_SYNTHESIS_SERVER_PATH: string
    // more env variables...
  }
  
  interface ImportMeta {
    readonly env: ImportMetaEnv
  }