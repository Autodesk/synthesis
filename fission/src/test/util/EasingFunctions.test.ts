import { describe, expect, test, vi } from "vitest"
import { easeOutQuad } from "@/util/EasingFunctions"

describe("Ease Out Quad Tests", () => {
    test("Whole Numbers", () => {
        expect(easeOutQuad(0)).toBeCloseTo(0)
        expect(easeOutQuad(1)).toBeCloseTo(1)
    })

    test("Fractions", () => {
        expect(easeOutQuad(0.5)).toBeCloseTo(0.75)
        expect(easeOutQuad(0.25)).toBeCloseTo(0.4375)
        expect(easeOutQuad(0.75)).toBeCloseTo(0.9375)
    })

    test("Within Range", () => {
        const steps = 100
        for (let i = 0; i <= steps; i++) {
            const n = i / steps
            const result = easeOutQuad(n)
            expect(result).toBeGreaterThanOrEqual(0)
            expect(result).toBeLessThanOrEqual(1)
        }
    })

    test("Out of Range Inputs", () => {
        const consoleErrorSpy = vi.spyOn(console, "error").mockImplementation(() => {})
        easeOutQuad(-0.1)
        easeOutQuad(1.1)
        easeOutQuad(2)
        easeOutQuad(-3)
        expect(consoleErrorSpy).toHaveBeenCalledTimes(4)
        consoleErrorSpy.mockRestore()
    })
})
