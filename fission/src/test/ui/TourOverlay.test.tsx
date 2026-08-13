import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { UIContext, type UIBlockState, type UIContextProps } from "@/ui/helpers/UIProviderHelpers"
import { TourContext, type TourContextValue } from "@/ui/tour/TourProviderHelpers"
import TourOverlay from "@/ui/tour/TourOverlay"
import { TOUR_STEPS } from "@/ui/tour/tourSteps"

const SCRIM_COLOR = "rgba(0, 0, 0, 0.5)"
const SCREEN_STEP = TOUR_STEPS.findIndex(step => step.focus === "screen")
const GATED_STEP = TOUR_STEPS.findIndex(step => step.advanceOn)

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

const tourContext = (stepIndex: number, canAdvance: boolean): TourContextValue => ({
    active: true,
    stepIndex,
    canAdvance,
    next: () => {},
    prev: () => {},
    skip: () => {},
    nudge: () => {},
    registerAnchor: () => {},
    getAnchor: () => null,
    anchorVersion: 0,
})

function renderOverlay({
    stepIndex,
    canAdvance = true,
    blockState = { blocked: false },
}: {
    stepIndex: number
    canAdvance?: boolean
    blockState?: UIBlockState
}) {
    return render(
        <UIContext.Provider value={uiContext(blockState)}>
            <TourContext.Provider value={tourContext(stepIndex, canAdvance)}>
                <TourOverlay />
            </TourContext.Provider>
        </UIContext.Provider>
    )
}

const scrimCount = (baseElement: HTMLElement) =>
    [...baseElement.querySelectorAll("div")].filter(el => getComputedStyle(el).backgroundColor === SCRIM_COLOR).length

describe("tour scrim", () => {
    test("scrims the screen on a focused step", () => {
        const { baseElement } = renderOverlay({ stepIndex: SCREEN_STEP })
        expect(scrimCount(baseElement)).toBeGreaterThan(0)
    })

    test("does not scrim while a blocking panel is open", () => {
        const { baseElement } = renderOverlay({
            stepIndex: SCREEN_STEP,
            blockState: { blocked: true, blockMessage: "Finish Assembly Setup first!" },
        })
        expect(scrimCount(baseElement)).toBe(0)
    })
})

describe("tour next control", () => {
    test("offers Next on a gated step once its condition is met", () => {
        const { getByLabelText } = renderOverlay({ stepIndex: GATED_STEP, canAdvance: true })
        expect(getByLabelText("Next step")).toHaveAttribute("aria-disabled", "false")
    })

    test("withholds Next on a gated step that is still waiting", () => {
        const { getByLabelText } = renderOverlay({ stepIndex: GATED_STEP, canAdvance: false })
        expect(getByLabelText("Next step")).toHaveAttribute("aria-disabled", "true")
    })
})
