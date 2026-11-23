declare interface Window {
    convertAuthToken(code: string, state: string | null): void
    gtag?: (command: "config" | "set" | "get" | "event" | "consent", ...args: unknown[]) => void
    dataLayer?: unknown[][]
    World?: unknown
}
