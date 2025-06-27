import type { Mock } from "vitest"

declare global {
    let gtag: Mock
    let URL: Mock
    let testProperty: string
}

export {}
