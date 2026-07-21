import { vi } from "vitest"

export function mockAnalytics(): void {
    vi.mock("@/systems/World", () => ({
        default: {
            get analyticsSystem() {
                return { event: vi.fn(), exception: vi.fn() }
            },
        },
    }))
}

export function mockConsole() {
    vi.spyOn(console, "log").mockReturnValue()
    vi.spyOn(console, "warn").mockReturnValue()
    vi.spyOn(console, "error").mockReturnValue()
    vi.spyOn(console, "debug").mockReturnValue()
}
