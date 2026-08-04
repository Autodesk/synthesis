import { describe, expect, test } from "vitest"
import { SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import {
    isScoreboardVisible,
    SCOREBOARD_GLYPH_SX,
    SCOREBOARD_MODE_LABELS,
    toggledScoreboardMode,
} from "@/ui/helpers/ScoreboardVisibility"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

const GAMEPLAY_STATES = [false, true]

describe("isScoreboardVisible", () => {
    test("auto follows gameplay", () => {
        expect(isScoreboardVisible("auto", true)).toBe(true)
        expect(isScoreboardVisible("auto", false)).toBe(false)
    })

    test("on and off ignore gameplay", () => {
        for (const gameplayActive of GAMEPLAY_STATES) {
            expect(isScoreboardVisible("on", gameplayActive)).toBe(true)
            expect(isScoreboardVisible("off", gameplayActive)).toBe(false)
        }
    })
})

describe("toggledScoreboardMode", () => {
    test("pins the scoreboard to the opposite of what is on screen", () => {
        for (const mode of SCOREBOARD_MODES) {
            for (const gameplayActive of GAMEPLAY_STATES) {
                const toggled = toggledScoreboardMode(mode, gameplayActive)

                expect(toggled).not.toBe("auto")
                expect(isScoreboardVisible(toggled, gameplayActive)).toBe(!isScoreboardVisible(mode, gameplayActive))
            }
        }
    })

    test("a choice made in gameplay survives leaving and returning to it", () => {
        const hidden = toggledScoreboardMode("auto", true)

        expect(isScoreboardVisible(hidden, false)).toBe(false)
        expect(isScoreboardVisible(hidden, true)).toBe(false)
    })

    test("toggling twice returns to the starting visibility", () => {
        for (const mode of SCOREBOARD_MODES) {
            for (const gameplayActive of GAMEPLAY_STATES) {
                const once = toggledScoreboardMode(mode, gameplayActive)
                const twice = toggledScoreboardMode(once, gameplayActive)

                expect(isScoreboardVisible(twice, gameplayActive)).toBe(isScoreboardVisible(mode, gameplayActive))
            }
        }
    })
})

describe("SCOREBOARD_MODE_LABELS", () => {
    test("every mode is labeled distinctly", () => {
        const labels = SCOREBOARD_MODES.map(mode => SCOREBOARD_MODE_LABELS[mode])

        expect(labels.filter(label => label.length > 0)).toHaveLength(SCOREBOARD_MODES.length)
        expect(new Set(labels).size).toBe(SCOREBOARD_MODES.length)
    })
})

describe("SCOREBOARD_GLYPH_SX", () => {
    test("every mode looks different in the top bar", () => {
        const styles = SCOREBOARD_MODES.map(mode => JSON.stringify(SCOREBOARD_GLYPH_SX[mode]))

        expect(new Set(styles).size).toBe(SCOREBOARD_MODES.length)
    })

    test("keeps the shared glyph sizing so the button matches its neighbors", () => {
        for (const mode of SCOREBOARD_MODES) {
            expect(SCOREBOARD_GLYPH_SX[mode]).toMatchObject(TOP_BAR_GLYPH_SX)
        }
    })

    test("only a mode that pins visibility departs from the top bar color", () => {
        expect(SCOREBOARD_GLYPH_SX.auto).toEqual(TOP_BAR_GLYPH_SX)
        expect(SCOREBOARD_GLYPH_SX.on).toHaveProperty("color")
        expect(SCOREBOARD_GLYPH_SX.off).toHaveProperty("opacity")
    })
})
