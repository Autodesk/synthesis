import { describe, expect, test } from "vitest"
import { isScoreboardVisible, type ScoreboardState, toggleScoreboard } from "@/ui/helpers/ScoreboardVisibility"

const UNTOUCHED: ScoreboardState = { preference: false, preferenceSet: false, suggestedByMode: false }

const apply = (state: ScoreboardState): ScoreboardState => ({
    ...state,
    preference: toggleScoreboard(state).preference,
    preferenceSet: true,
})

describe("isScoreboardVisible", () => {
    test("stays hidden outside gameplay until the user asks for it", () => {
        expect(isScoreboardVisible(UNTOUCHED)).toBe(false)
        expect(isScoreboardVisible({ ...UNTOUCHED, preference: true })).toBe(true)
    })

    test("shows during gameplay and match mode for a user who has never chosen", () => {
        expect(isScoreboardVisible({ ...UNTOUCHED, suggestedByMode: true })).toBe(true)
    })

    test("lets an explicit choice override what the mode suggests", () => {
        expect(isScoreboardVisible({ preference: false, preferenceSet: true, suggestedByMode: true })).toBe(false)
        expect(isScoreboardVisible({ preference: true, preferenceSet: true, suggestedByMode: false })).toBe(true)
    })
})

describe("toggleScoreboard", () => {
    test("the first press in gameplay keeps it visible and announces the saved preference", () => {
        const gameplay = { ...UNTOUCHED, suggestedByMode: true }
        expect(isScoreboardVisible(gameplay)).toBe(true)

        const result = toggleScoreboard(gameplay)

        expect(result).toEqual({ preference: true, announcePreference: true })
        expect(isScoreboardVisible(apply(gameplay))).toBe(true)
    })

    test("presses after that flip visibility without announcing anything", () => {
        let state = apply({ ...UNTOUCHED, suggestedByMode: true })

        expect(toggleScoreboard(state).announcePreference).toBe(false)
        state = apply(state)
        expect(isScoreboardVisible(state)).toBe(false)

        expect(toggleScoreboard(state).announcePreference).toBe(false)
        state = apply(state)
        expect(isScoreboardVisible(state)).toBe(true)
    })

    test("never announces when preference and visibility already agree", () => {
        for (const preference of [false, true]) {
            for (const suggestedByMode of [false, true]) {
                const state = { preference, preferenceSet: true, suggestedByMode }
                expect(toggleScoreboard(state).announcePreference).toBe(false)
                expect(isScoreboardVisible(apply(state))).toBe(!preference)
            }
        }
    })

    test("a choice made in gameplay survives a reload back into gameplay", () => {
        const hidden = apply(apply({ ...UNTOUCHED, suggestedByMode: true }))
        expect(hidden.preference).toBe(false)

        const reloaded: ScoreboardState = { ...hidden, suggestedByMode: true }

        expect(isScoreboardVisible(reloaded)).toBe(false)
    })
})
