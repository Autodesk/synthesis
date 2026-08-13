import { fireEvent, render, screen, waitFor, within } from "@testing-library/react"
import { act } from "react"
import { beforeEach, describe, expect, test } from "vitest"
import EventSystem from "@/systems/EventSystem"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Scoreboard from "@/ui/components/Scoreboard"
import GameplayControls from "@/ui/components/topbar/GameplayControls"

const isShown = (container: HTMLElement) => container.firstChild !== null

const setAlwaysShow = (value: boolean) => act(() => PreferencesSystem.setUserPreference("AlwaysShowScoreboard", value))

const setMatchMode = (mode: MatchModeType) => act(() => EventSystem.dispatch("MatchStateChangedEvent", { mode }))

describe("Scoreboard", () => {
    beforeEach(() => setAlwaysShow(false))

    test("only follows match state when it is not set to always show", async () => {
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

describe("ScoreboardButton", () => {
    const button = () => within(screen.getByLabelText(/^Scoreboard:/)).getByRole("button")

    beforeEach(() => setAlwaysShow(false))

    test("reports the current mode and toggles the preference when clicked", async () => {
        render(<GameplayControls />)

        expect(screen.getByLabelText("Scoreboard: Only During Matches")).toBeInTheDocument()
        expect(button()).toHaveAttribute("aria-pressed", "false")

        fireEvent.click(button())

        expect(PreferencesSystem.getUserPreference("AlwaysShowScoreboard")).toBe(true)
        await waitFor(() => expect(screen.getByLabelText("Scoreboard: Always On")).toBeInTheDocument())
        expect(button()).toHaveAttribute("aria-pressed", "true")
    })

    test("follows the preference when it changes elsewhere", async () => {
        render(<GameplayControls />)

        setAlwaysShow(true)

        await waitFor(() => expect(button()).toHaveAttribute("aria-pressed", "true"))
    })
})
