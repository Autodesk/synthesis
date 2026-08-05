import { expect, test } from "vitest"
import { SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { isScoreboardVisible, toggledScoreboardMode } from "@/ui/helpers/ScoreboardVisibility"

test("only auto depends on gameplay", () => {
    expect(isScoreboardVisible("auto", false)).toBe(false)
    expect(isScoreboardVisible("auto", true)).toBe(true)
    expect(isScoreboardVisible("on", false)).toBe(true)
    expect(isScoreboardVisible("on", true)).toBe(true)
    expect(isScoreboardVisible("off", false)).toBe(false)
    expect(isScoreboardVisible("off", true)).toBe(false)
})

test("toggling pins the scoreboard to the opposite of what is on screen", () => {
    for (const mode of SCOREBOARD_MODES) {
        for (const gameplayActive of [false, true]) {
            const toggled = toggledScoreboardMode(mode, gameplayActive)

            expect(toggled).not.toBe("auto")
            expect(isScoreboardVisible(toggled, gameplayActive)).toBe(!isScoreboardVisible(mode, gameplayActive))
        }
    }
})
