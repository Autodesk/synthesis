import { expect, test } from "vitest"
import { type ScoreboardMode, SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { isScoreboardVisible, nextScoreboardMode } from "@/ui/helpers/ScoreboardVisibility"

test("only auto depends on gameplay", () => {
    expect(isScoreboardVisible("auto", false)).toBe(false)
    expect(isScoreboardVisible("auto", true)).toBe(true)
    expect(isScoreboardVisible("on", false)).toBe(true)
    expect(isScoreboardVisible("on", true)).toBe(true)
    expect(isScoreboardVisible("off", false)).toBe(false)
    expect(isScoreboardVisible("off", true)).toBe(false)
})

test("a running match shows the scoreboard in every mode", () => {
    for (const mode of SCOREBOARD_MODES) {
        expect(isScoreboardVisible(mode, true, true)).toBe(true)
    }
})

test("cycling visits every mode and returns to the start", () => {
    let mode: ScoreboardMode = SCOREBOARD_MODES[0]
    const visited = SCOREBOARD_MODES.map(() => (mode = nextScoreboardMode(mode)))

    expect(new Set(visited).size).toBe(SCOREBOARD_MODES.length)
    expect(mode).toBe(SCOREBOARD_MODES[0])
})
