import { beforeEach, describe, expect, test, vi } from "vitest"
import { DOMUnit, DOMUnitExpression } from "@/util/Units"

vi.mock("@/util/Utility", () => {
    const original = vi.importActual<typeof import("@/util/Utility")>("@/util/Utility")
    return { ...original, getFontSize: vi.fn(() => 16) }
})

describe("DOMUnit", () => {
    let mockElement: HTMLElement

    beforeEach(async () => {
        mockElement = document.createElement("div")
        Object.defineProperty(mockElement, "clientWidth", { value: 400 })
        Object.defineProperty(mockElement, "clientHeight", { value: 200 })
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

describe("DOMUnitExpression", () => {
    let mockElement: HTMLElement

    beforeEach(() => {
        mockElement = document.createElement("div")
        Object.defineProperty(mockElement, "clientWidth", { value: 400 })
        Object.defineProperty(mockElement, "clientHeight", { value: 200 })
    })

    test("Evaluates Single Unit Expression Correctly", () => {
        const result = DOMUnitExpression.fromUnit(42, "px").evaluate(mockElement)
        expect(result).toBe(42)
    })

    test("Evaluates Addition of Two PX Units Correctly", () => {
        const result = DOMUnitExpression.fromUnit(20, "px").add(DOMUnitExpression.fromUnit(22, "px"))
        expect(result.evaluate(mockElement)).toBe(42)
    })

    test("Evaluates Addition of PX and REM Units Correctly", () => {
        const expr = DOMUnitExpression.fromUnit(16, "px").add(DOMUnitExpression.fromUnit(1, "rem"))
        expect(expr.evaluate(mockElement)).toBe(32)
    })

    test("Evaluates Subtraction of PX Units Correctly", () => {
        const expr = DOMUnitExpression.fromUnit(50, "px").sub(DOMUnitExpression.fromUnit(8, "px"))
        expect(expr.evaluate(mockElement)).toBe(42)
    })

    test("Evaluates Multiplication of PX Unit by Scalar", () => {
        const expr = DOMUnitExpression.fromUnit(7, "px").mul(DOMUnitExpression.fromUnit(6, "px"))
        expect(expr.evaluate(mockElement)).toBe(42)
    })

    test("Evaluate Multiplication of PX Unit by Decimal", () => {
        const expr = DOMUnitExpression.fromUnit(84, "px").mul(DOMUnitExpression.fromUnit(0.5, "px"))
        expect(expr.evaluate(mockElement)).toBe(42)
    })

    test("Evaluates Division of PX Unit by Scalar", () => {
        const expr = DOMUnitExpression.fromUnit(84, "px").div(DOMUnitExpression.fromUnit(2, "px"))
        expect(expr.evaluate(mockElement)).toBe(42)
    })

    test("Evaluates Addition of Width (W) and Height (H) Units Correctly", () => {
        const expr = DOMUnitExpression.fromUnit(0.1, "w").add(DOMUnitExpression.fromUnit(0.11, "h"))
        // 0.1 * 400 + 0.11 * 200 = 40 + 22 = 62
        expect(expr.evaluate(mockElement)).toBe(62)
    })

    test("Evaluates Subtraction of Width (W) and Height (H) Units Correctly", () => {
        const expr = DOMUnitExpression.fromUnit(0.5, "w").sub(DOMUnitExpression.fromUnit(0.2, "h"))
        // 0.5 * 400 - 0.2 * 200 = 200 - 40 = 160
        expect(expr.evaluate(mockElement)).toBe(160)
    })

    test("Evaluates Subtraction of REM and PX Units Correctly", () => {
        const expr = DOMUnitExpression.fromUnit(3, "rem").sub(DOMUnitExpression.fromUnit(6, "px"))
        // 3 * 16 - 6 = 48 - 6 = 42
        expect(expr.evaluate(mockElement)).toBe(42)
    })
})
