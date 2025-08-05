import { describe, expect, test } from "vitest"
import * as Random from "../../util/Random"

describe("Random Number Generator Tests", () => {
    test("Range Compliance", () => {
        Random.seedRandomGen(67542431)
        for (let i = 0; i < 99; i++) {
            const a = Random.random()
            expect(a).toBeLessThan(1.0)
            expect(a).toBeGreaterThanOrEqual(0.0)
        }
    })
})
