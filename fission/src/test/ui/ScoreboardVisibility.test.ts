import { describe, expect, test } from "vitest"
import { SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { isScoreboardVisible, SCOREBOARD_MODE_LABELS, toggledScoreboardMode } from "@/ui/helpers/ScoreboardVisibility"

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
