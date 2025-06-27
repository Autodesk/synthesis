import type { Mock } from "vitest"

declare global {
    var gtag: Mock
    var testProperty: string
}

export {}
