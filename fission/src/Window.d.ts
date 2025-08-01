declare interface Window {
    convertAuthToken(code: string): void
    gtag: () => void
    world?: unknown // for development
}
