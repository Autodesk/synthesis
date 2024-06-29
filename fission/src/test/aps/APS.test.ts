import APS from "@/aps/APS"
import { describe, expect, test } from "vitest"

describe("APS", () => {
    test("Generate Random Strings (10)", () => {
        const s = APS.genRandomString(10)

        ;(async () => {
            const [v, c] = await APS.codeChallenge()
            console.log(`${v}`)
            console.log(`${c}`)
        })()

        expect(s.length).toBe(10)
        const matches = s.match(/^([0-9a-z])*$/i)
        expect(matches).toBeDefined()
        expect(matches!.length).toBeGreaterThanOrEqual(1)
        expect(matches![0]).toBe(s)
    })

    test("Generate Random Strings (50)", () => {
        const s = APS.genRandomString(50)

        expect(s.length).toBe(50)
        const matches = s.match(/^([0-9a-z])*$/i)
        expect(matches).toBeDefined()
        expect(matches!.length).toBeGreaterThanOrEqual(1)
        expect(matches![0]).toBe(s)
    })

    test("Generate Random Strings (75)", () => {
        const s = APS.genRandomString(75)

        expect(s.length).toBe(75)
        const matches = s.match(/^([0-9a-z])*$/i)
        expect(matches).toBeDefined()
        expect(matches!.length).toBeGreaterThanOrEqual(1)
        expect(matches![0]).toBe(s)
    })
})
