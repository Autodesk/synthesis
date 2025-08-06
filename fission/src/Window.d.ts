declare interface Window {
    convertAuthToken(code: string): void
    world?: unknown // for development
    gtag?: (command: "config" | "set" | "get" | "event" | "consent", ...args: unknown[]) => void
    dataLayer?: unknown[][]
}
