import { describe, expect, test } from "vitest"
import { computeVisibleCount } from "@/ui/components/topbar/TopBarFit"

const GAP = 12
const BUTTON = 40
const SLACK = 8

const BUTTONS = Array<number>(6).fill(BUTTON)

const budgetFor = (n: number) => n * (BUTTON + GAP) + SLACK

describe("computeVisibleCount", () => {
    test("shows everything when there is room to spare", () => {
        expect(computeVisibleCount(budgetFor(6) + 100, BUTTONS, GAP)).toBe(6)
    })

    test("shows everything at exactly the budget it needs", () => {
        expect(computeVisibleCount(budgetFor(6), BUTTONS, GAP)).toBe(6)
    })

    test("holds back slack so a sub-pixel measurement error cannot overflow the row", () => {
        expect(computeVisibleCount(budgetFor(6) - 1, BUTTONS, GAP)).toBe(5)
        expect(computeVisibleCount(budgetFor(6) - SLACK, BUTTONS, GAP)).toBe(5)
    })

    test("drops one item at a time as the budget shrinks", () => {
        for (let visible = 0; visible <= 6; visible++) {
            expect(computeVisibleCount(budgetFor(visible), BUTTONS, GAP)).toBe(visible)
            expect(computeVisibleCount(budgetFor(visible + 1) - 1, BUTTONS, GAP)).toBe(visible)
        }
    })

    test("drops from the end, so the front of the list is the last to go", () => {
        const widths = [BUTTON, BUTTON, 200]

        expect(computeVisibleCount(budgetFor(2) + 200 + GAP, widths, GAP)).toBe(3)
        expect(computeVisibleCount(budgetFor(2), widths, GAP)).toBe(2)
        expect(computeVisibleCount(budgetFor(1), widths, GAP)).toBe(1)
    })

    test("stops at an item too wide to fit rather than skipping past it", () => {
        expect(computeVisibleCount(budgetFor(3), [200, BUTTON, BUTTON, BUTTON], GAP)).toBe(0)
    })

    test("shows nothing when the budget is gone or negative", () => {
        expect(computeVisibleCount(0, BUTTONS, GAP)).toBe(0)
        expect(computeVisibleCount(-500, BUTTONS, GAP)).toBe(0)
    })

    test("handles an empty item list", () => {
        expect(computeVisibleCount(1000, [], GAP)).toBe(0)
    })
})
