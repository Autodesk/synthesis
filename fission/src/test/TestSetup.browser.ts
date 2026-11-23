import { vi } from "vitest"

vi.mock("console-prefixer", () => ({
    consolePrefixer: vi.fn().mockReturnValue(console),
}))
