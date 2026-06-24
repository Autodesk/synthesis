import { describe, expect, test } from "vitest"
import { shortestAngleDelta, slewTowards } from "@/systems/simulation/driver/AngleUtil"

describe("slewTowards", () => {
    test("returns the target exactly when it is within maxDelta", () => {
        expect(slewTowards(0, 0.3, 0.5)).toBe(0.3)
        expect(slewTowards(1.0, 0.8, 0.5)).toBe(0.8)
        expect(slewTowards(2, 2, 0.5)).toBe(2)
    })

    test("moves by at most maxDelta toward a far positive target", () => {
        expect(slewTowards(0, 3.14, 0.5)).toBeCloseTo(0.5, 10)
    })

    test("moves by at most maxDelta toward a far negative target", () => {
        expect(slewTowards(0, -3.14, 0.5)).toBeCloseTo(-0.5, 10)
    })

    test("ramps to a far target over multiple steps and then settles", () => {
        let angle = 0
        const target = 1.6
        const maxDelta = 0.5
        const seen: number[] = []
        for (let i = 0; i < 6; i++) {
            angle = slewTowards(angle, target, maxDelta)
            seen.push(Number(angle.toFixed(3)))
        }
        // 0 -> 0.5 -> 1.0 -> 1.5 -> 1.6 (settle) -> 1.6 -> 1.6
        expect(seen).toEqual([0.5, 1.0, 1.5, 1.6, 1.6, 1.6])
    })

    test("never overshoots regardless of step count", () => {
        let angle = 0
        for (let i = 0; i < 100; i++) angle = slewTowards(angle, -2.5, 0.1)
        expect(angle).toBeCloseTo(-2.5, 10)
    })

    test("maxDelta of 0 (or negative) holds the current value", () => {
        expect(slewTowards(0.7, 3.0, 0)).toBe(0.7)
        expect(slewTowards(0.7, 3.0, -1)).toBe(0.7)
    })
})

describe("shortestAngleDelta", () => {
    const PI = Math.PI

    test("returns the plain difference when no wrap is needed", () => {
        expect(shortestAngleDelta(0, 1)).toBeCloseTo(1, 10)
        expect(shortestAngleDelta(1, 0.5)).toBeCloseTo(-0.5, 10)
        expect(shortestAngleDelta(0, 0)).toBe(0)
    })

    test("takes the short way across the +π/-π seam instead of the long way", () => {
        // The exact bug case from the log: module at +2.263 rad, target -2.907 rad. Naive
        // difference is -5.17 (the long way); shortest path is +1.11 across the seam.
        expect(shortestAngleDelta(2.263, -2.907)).toBeCloseTo(1.113, 3)
    })

    test("never returns a rotation larger than π in magnitude", () => {
        for (let from = -PI; from <= PI; from += 0.31) {
            for (let to = -PI; to <= PI; to += 0.29) {
                expect(Math.abs(shortestAngleDelta(from, to))).toBeLessThanOrEqual(PI + 1e-9)
            }
        }
    })

    test("applying the delta lands on the target (mod 2π)", () => {
        const cases: [number, number][] = [
            [2.263, -2.907],
            [-3.0, 3.0],
            [0.1, -0.1],
            [PI - 0.05, -PI + 0.05],
        ]
        for (const [from, to] of cases) {
            const landed = from + shortestAngleDelta(from, to)
            const diff = Math.abs((((landed - to) % (2 * PI)) + 2 * PI) % (2 * PI))
            expect(Math.min(diff, 2 * PI - diff)).toBeLessThan(1e-9)
        }
    })
})
