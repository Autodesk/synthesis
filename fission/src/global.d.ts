import type { Mock } from "vitest"

declare global {
    interface Global {
        gtag: Mock
        URL: Mock & typeof URL
        testProperty: string
    }
}

export {}
