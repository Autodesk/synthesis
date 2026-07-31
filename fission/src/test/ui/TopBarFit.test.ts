import { describe, expect, test } from "vitest"
import { computeVisibleCount } from "@/ui/components/topbar/TopBarFit"

const GAP = 12
const BUTTON = 40

const BUTTONS = Array<number>(6).fill(BUTTON)

const widthOf = (n: number) => n * (BUTTON + GAP)

describe("computeVisibleCount", () => {
    test("shows everything when there is room to spare", () => {
        expect(computeVisibleCount(widthOf(6) + 100, BUTTONS, GAP)).toBe(6)
    })

    test("shows everything at exactly the width it needs", () => {
        expect(computeVisibleCount(widthOf(6), BUTTONS, GAP)).toBe(6)
    })

    test("drops one item at a time as the budget shrinks", () => {
        for (let visible = 0; visible <= 6; visible++) {
            expect(computeVisibleCount(widthOf(visible), BUTTONS, GAP)).toBe(visible)
            expect(computeVisibleCount(widthOf(visible + 1) - 1, BUTTONS, GAP)).toBe(visible)
        }
    })

    test("drops from the end, so the front of the list is the last to go", () => {
        expect(computeVisibleCount(widthOf(3), [200, BUTTON, BUTTON, BUTTON], GAP)).toBe(0)
    })

    test("shows nothing when the budget is gone or negative", () => {
        expect(computeVisibleCount(0, BUTTONS, GAP)).toBe(0)
        expect(computeVisibleCount(-500, BUTTONS, GAP)).toBe(0)
    })

    test("handles an empty item list", () => {
        expect(computeVisibleCount(1000, [], GAP)).toBe(0)
    })
})
