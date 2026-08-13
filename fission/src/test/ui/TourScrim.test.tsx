import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { UIContext, type UIBlockState, type UIContextProps } from "@/ui/helpers/UIProviderHelpers"
import { TourContext, type TourContextValue } from "@/ui/tour/TourProviderHelpers"
import TourOverlay from "@/ui/tour/TourOverlay"
import { TOUR_STEPS } from "@/ui/tour/tourSteps"

const uiContext = (blockState: UIBlockState): UIContextProps => ({
    panels: [],
    blockState,
    openModal: () => null,
    openPanel: () => null,
    togglePanel: () => null,
    closeModal: () => {},
    closePanel: () => {},
    addToast: () => {},
    configureScreen: () => {},
})

const SCREEN_STEP = TOUR_STEPS.findIndex(step => step.focus === "screen")

const tourContext: TourContextValue = {
    active: true,
    stepIndex: SCREEN_STEP,
    next: () => {},
    prev: () => {},
    skip: () => {},
    nudge: () => {},
    registerAnchor: () => {},
    getAnchor: () => null,
    anchorVersion: 0,
}

const SCRIM_COLOR = "rgba(0, 0, 0, 0.5)"

function scrimCount(blockState: UIBlockState) {
    const { baseElement } = render(
        <UIContext.Provider value={uiContext(blockState)}>
            <TourContext.Provider value={tourContext}>
                <TourOverlay />
            </TourContext.Provider>
        </UIContext.Provider>
    )
    return [...baseElement.querySelectorAll("div")].filter(el => getComputedStyle(el).backgroundColor === SCRIM_COLOR)
        .length
}

describe("tour scrim", () => {
    test("scrims the screen on a focused step", () => {
        expect(scrimCount({ blocked: false })).toBeGreaterThan(0)
    })

    test("does not scrim while a blocking panel is open", () => {
        expect(scrimCount({ blocked: true, blockMessage: "Finish Assembly Setup first!" })).toBe(0)
    })
})
