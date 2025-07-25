import { describe, expect, test } from "vitest"
import { clamp } from "@/util/Utility"

describe("Clamp Tests", () => {
    test("Clamp Within Range", () => {
        expect(clamp(5, 0, 10)).toBe(5)
        expect(clamp(-3, -5, 5)).toBe(-3)
        expect(clamp(7.5, 0, 10)).toBe(7.5)
    })

    test("Clamp Below Minimum", () => {
        expect(clamp(-10, 0, 10)).toBe(0)
        expect(clamp(-6, -5, 5)).toBe(-5)
        expect(clamp(9.5, 9.7, 11)).toBe(9.7)
    })

    test("Clamp Above Maximum", () => {
        expect(clamp(15, 0, 10)).toBe(10)
        expect(clamp(6, -5, 5)).toBe(5)
        expect(clamp(-12.3, -17, -13.4)).toBe(-13.4)
    })

    test("Clamp at Boundaries", () => {
        expect(clamp(8, 8, 8)).toBe(8)
        expect(clamp(0, 0, 10)).toBe(0)
        expect(clamp(10, 0, 10)).toBe(10)
        expect(clamp(-5, -5, 5)).toBe(-5)
        expect(clamp(5, -5, 5)).toBe(5)
        expect(clamp(8.4, 8.4, 60)).toBe(8.4)
    })
})
