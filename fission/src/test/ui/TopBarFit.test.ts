import { describe, expect, test } from "vitest"
import { computeVisibleCount, FIT_TOLERANCE_PX } from "@/ui/components/topbar/TopBarFit"

const GAP = 12
const BUTTON = 40

const BUTTONS = Array<number>(6).fill(BUTTON)

const leastBudgetFitting = (n: number) => n * (BUTTON + GAP) + FIT_TOLERANCE_PX

describe("computeVisibleCount", () => {
    test("drops exactly one item per item-width of budget, holding back slack", () => {
        for (let visible = 0; visible <= BUTTONS.length; visible++) {
            expect(computeVisibleCount(leastBudgetFitting(visible), BUTTONS, GAP)).toBe(visible)
            expect(computeVisibleCount(leastBudgetFitting(visible + 1) - 1, BUTTONS, GAP)).toBe(visible)
        }
    })

    test("stops at an item too wide to fit rather than skipping past it", () => {
        expect(computeVisibleCount(leastBudgetFitting(3), [200, BUTTON, BUTTON, BUTTON], GAP)).toBe(0)
    })

    test("shows nothing for an exhausted budget or an empty list", () => {
        expect(computeVisibleCount(0, BUTTONS, GAP)).toBe(0)
        expect(computeVisibleCount(-500, BUTTONS, GAP)).toBe(0)
        expect(computeVisibleCount(1000, [], GAP)).toBe(0)
    })
})
