import { test, expect, describe, vi, beforeEach } from "vitest"
import { DOMUnit, DOMUnitExpression } from "@/util/Units"

describe("DOMUnit", () => {
    let mockElement: HTMLElement

    beforeEach(async () => {
        mockElement = document.createElement("div")
        Object.defineProperty(mockElement, "clientWidth", { value: 400 })
        Object.defineProperty(mockElement, "clientHeight", { value: 200 })

        vi.mock("@/util/Utility", () => {
            const original = vi.importActual<typeof import("@/util/Utility")>("@/util/Utility")
            return { ...original, getFontSize: vi.fn(() => 16) }
        })
    })

    test("Evaluates PX Units Correctly", () => {
        const unit = new DOMUnit(42, "px")
        expect(unit.evaluate(mockElement)).toBe(42)
    })

    test("Evaluates REM Units Correctly", () => {
        const unit = new DOMUnit(2, "rem")
        expect(unit.evaluate(mockElement)).toBe(32)
    })

    test("Evaluates Width (W) Units Correctly", () => {
        const unit = new DOMUnit(0.5, "w")
        expect(unit.evaluate(mockElement)).toBe(200)
    })

    test("Evaluates Height (H) Units Correctly", () => {
        const unit = new DOMUnit(0.2, "h")
        expect(unit.evaluate(mockElement)).toBe(40)
    })
})
