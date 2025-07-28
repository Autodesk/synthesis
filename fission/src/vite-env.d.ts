/// <reference types="vite/client" />

// Electron Forge Vite plugin environment variables
declare const MAIN_WINDOW_VITE_DEV_SERVER_URL: string | undefined;
declare const MAIN_WINDOW_VITE_NAME: string;

// Type declaration for electron-squirrel-startup
declare module 'electron-squirrel-startup' {
  const started: boolean;
  export = started;
}

interface Window {
  electronAPI?: {
    platform: string;
  };
}
