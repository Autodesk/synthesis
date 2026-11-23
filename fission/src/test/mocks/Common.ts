import { vi } from "vitest"

export function mockConsole() {
    vi.spyOn(console, "log").mockReturnValue()
    vi.spyOn(console, "warn").mockReturnValue()
    vi.spyOn(console, "error").mockReturnValue()
    vi.spyOn(console, "debug").mockReturnValue()
}
