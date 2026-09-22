import { fireEvent, render, screen, waitFor } from "@testing-library/react"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ConfigurationSubpanelProps } from "@/panels/configuring/assembly-config/ConfigTypes"
import { ThemeProvider } from "@/ui/ThemeProvider"
import AllianceSelectionInterface from "@/ui/panels/configuring/assembly-config/interfaces/AllianceSelectionInterface"

const RED_ALLIANCE_COLOR = "#cf5b3b"
const BLUE_ALLIANCE_COLOR = "#167d9a"

describe("AllianceSelectionInterface", () => {
    beforeEach(() => {
        localStorage.setItem(
            "theme",
            JSON.stringify({
                primary: { main: "#90caf9" },
                secondary: { main: "#ce93d8" },
                blueAlliance: { main: BLUE_ALLIANCE_COLOR },
                redAlliance: { main: RED_ALLIANCE_COLOR },
                topBar: { main: "#3d4352" },
                surface: { main: "#2a3340" },
                topBarText: { main: "#bfc5ce" },
            })
        )
    })

    afterEach(() => {
        localStorage.removeItem("theme")
    })

    test("uses the configured alliance color for the alliance and selected station", async () => {
        const props = {
            selectedAssembly: { alliance: "red", station: 1 } as MirabufSceneObject,
            registerCleanupFunction: vi.fn(),
        } as unknown as ConfigurationSubpanelProps

        render(
            <ThemeProvider>
                <AllianceSelectionInterface {...props} />
            </ThemeProvider>
        )

        const allianceButton = screen.getByRole("button", { name: "Red Alliance" })
        const selectedStation = screen.getByRole("button", { name: "1" })

        expect(getComputedStyle(allianceButton).backgroundColor).toBe("rgb(207, 91, 59)")
        expect(getComputedStyle(selectedStation).backgroundColor).toBe("rgb(207, 91, 59)")

        fireEvent.click(allianceButton)

        const blueAllianceButton = screen.getByRole("button", { name: "Blue Alliance" })
        await waitFor(() => {
            expect(getComputedStyle(blueAllianceButton).backgroundColor).toBe("rgb(22, 125, 154)")
            expect(getComputedStyle(screen.getByRole("button", { name: "1" })).backgroundColor).toBe(
                "rgb(22, 125, 154)"
            )
        })
    })
})
