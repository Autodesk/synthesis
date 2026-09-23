import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { UIContext, type UIBlockState, type UIContextProps } from "@/ui/helpers/UIProviderHelpers"
import { TourContext, type TourContextValue } from "@/ui/tour/TourProviderHelpers"
import { visibleRect } from "@/ui/tour/AnchorGeometry"
import TourOverlay from "@/ui/tour/TourOverlay"
import { TOUR_STEPS } from "@/ui/tour/TourSteps"

const SCRIM_TEST_ID = "tour-scrim"
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
    registerAnchor: () => {},
    getAnchor: () => null,
    anchorVersion: 0,
})

const overlay = ({
    stepIndex,
    canAdvance = true,
    blockState = { blocked: false },
}: {
    stepIndex: number
    canAdvance?: boolean
    blockState?: UIBlockState
}) => (
    <UIContext.Provider value={uiContext(blockState)}>
        <TourContext.Provider value={tourContext(stepIndex, canAdvance)}>
            <TourOverlay />
        </TourContext.Provider>
    </UIContext.Provider>
)

describe("tour scrim", () => {
    test("scrims the screen on a focused step", () => {
        const { queryAllByTestId } = render(overlay({ stepIndex: SCREEN_STEP }))
        expect(queryAllByTestId(SCRIM_TEST_ID)).not.toHaveLength(0)
    })

    test("does not scrim while a blocking panel is open", () => {
        const { queryAllByTestId } = render(
            overlay({
                stepIndex: SCREEN_STEP,
                blockState: { blocked: true, blockMessage: "Finish Assembly Setup first!" },
            })
        )
        expect(queryAllByTestId(SCRIM_TEST_ID)).toHaveLength(0)
    })
})

describe("tour anchor geometry", () => {
    test("measures an anchor as the part its scroll container shows", () => {
        const container = document.createElement("div")
        container.style.cssText =
            "position: fixed; top: 100px; left: 50px; width: 300px; height: 100px; overflow: auto;"
        const anchor = document.createElement("div")
        anchor.style.cssText = "height: 1000px;"
        container.appendChild(anchor)
        document.body.appendChild(container)

        try {
            const rect = visibleRect(anchor)!
            expect(rect.top).toBeCloseTo(100, 0)
            expect(rect.height).toBeCloseTo(100, 0)
        } finally {
            container.remove()
        }
    })
})

describe("tour next control", () => {
    test("unlocks Next once a gated step's condition is met", () => {
        const { getByLabelText, rerender } = render(overlay({ stepIndex: GATED_STEP, canAdvance: false }))
        expect(getByLabelText("Next step")).toBeDisabled()

        rerender(overlay({ stepIndex: GATED_STEP, canAdvance: true }))
        expect(getByLabelText("Next step")).not.toBeDisabled()
    })
})
