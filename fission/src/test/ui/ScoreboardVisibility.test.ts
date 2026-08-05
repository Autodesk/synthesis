import { describe, expect, test } from "vitest"
import { SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { isScoreboardVisible, SCOREBOARD_GLYPH_SX, toggledScoreboardMode } from "@/ui/helpers/ScoreboardVisibility"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

const GAMEPLAY_ACTIVE_VALUES = [false, true]

describe("isScoreboardVisible", () => {
    test("auto follows gameplay", () => {
        expect(isScoreboardVisible("auto", true)).toBe(true)
        expect(isScoreboardVisible("auto", false)).toBe(false)
    })

    test("on and off ignore gameplay", () => {
        for (const gameplayActive of GAMEPLAY_ACTIVE_VALUES) {
            expect(isScoreboardVisible("on", gameplayActive)).toBe(true)
            expect(isScoreboardVisible("off", gameplayActive)).toBe(false)
        }
    })
})

describe("toggledScoreboardMode", () => {
    test("pins the scoreboard to the opposite of what is on screen", () => {
        for (const mode of SCOREBOARD_MODES) {
            for (const gameplayActive of GAMEPLAY_ACTIVE_VALUES) {
                const toggled = toggledScoreboardMode(mode, gameplayActive)

                expect(toggled).not.toBe("auto")
                expect(isScoreboardVisible(toggled, gameplayActive)).toBe(!isScoreboardVisible(mode, gameplayActive))
            }
        }
    })
})

describe("SCOREBOARD_GLYPH_SX", () => {
    test("every mode looks different while keeping the shared glyph sizing", () => {
        const styles = SCOREBOARD_MODES.map(mode => SCOREBOARD_GLYPH_SX[mode])

        for (const style of styles) expect(style).toMatchObject(TOP_BAR_GLYPH_SX)
        expect(new Set(styles.map(style => JSON.stringify(style))).size).toBe(SCOREBOARD_MODES.length)
    })
})
