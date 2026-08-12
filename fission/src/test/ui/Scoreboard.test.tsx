import { render, waitFor } from "@testing-library/react"
import { act } from "react"
import { describe, expect, test } from "vitest"
import EventSystem from "@/systems/EventSystem"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Scoreboard from "@/ui/components/Scoreboard"

const isShown = (container: HTMLElement) => container.textContent?.includes("RED") === true

const setAlwaysShow = (value: boolean) => act(() => PreferencesSystem.setUserPreference("AlwaysShowScoreboard", value))

const setMatchMode = (mode: MatchModeType) => act(() => EventSystem.dispatch("MatchStateChangedEvent", { mode }))

describe("Scoreboard", () => {
    test("only follows match state when it is not set to always show", async () => {
        setAlwaysShow(false)
        const { container } = render(<Scoreboard />)
        expect(isShown(container)).toBe(false)

        setMatchMode(MatchModeType.TELEOP)
        await waitFor(() => expect(isShown(container)).toBe(true))

        setMatchMode(MatchModeType.SANDBOX)
        await waitFor(() => expect(isShown(container)).toBe(false))
    })

    test("stays shown outside of a match when set to always show", async () => {
        setAlwaysShow(true)
        const { container } = render(<Scoreboard />)
        expect(isShown(container)).toBe(true)

        setAlwaysShow(false)
        await waitFor(() => expect(isShown(container)).toBe(false))
    })
})
