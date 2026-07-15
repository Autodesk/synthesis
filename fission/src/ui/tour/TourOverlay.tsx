import { Box, Popper } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { TOUR_STEPS } from "./tourSteps"
import TourCard from "./TourCard"
import { useTourContext } from "./TourProviderHelpers"

const ZIndex = 1400 // above panels/modals (1300) and the top bar (1200)

/** Maps a Popper placement to the card edge its pointer should sit on. */
function arrowEdgeFor(placement: string): "top" | "bottom" | "left" | "right" {
    const base = placement.split("-")[0]
    switch (base) {
        case "top":
            return "bottom"
        case "left":
            return "right"
        case "right":
            return "left"
        default:
            return "top" // bottom placement -> pointer on the card's top edge
    }
}

/**
 * Renders the current tour step's card, anchored to its registered element via an MUI
 * Popper (which repositions on resize, so cards stay attached across screen sizes).
 * Steps with no anchor - or whose anchor is not yet mounted - fall back to a centered card.
 */
const TourOverlay: React.FC = () => {
    // Consuming the context re-renders this component whenever the provider value changes -
    // including the anchorVersion bump on anchor (de)registration - so the anchor below is
    // always re-resolved when a panel mounts or unmounts.
    const { active, stepIndex, next, prev, skip, getAnchor } = useTourContext()
    const [arrowRef, setArrowRef] = useState<HTMLElement | null>(null)

    const step = active ? TOUR_STEPS[stepIndex] : undefined
    if (!step) return null

    // Gate the `>` button on steps that advance only when the user performs the real action.
    const nextDisabled = !!step.advanceOn

    // Resolved on every render; the context change from anchor (de)registration drives re-renders.
    // Guard on `isConnected`: while an anchor's host (a panel/modal) unmounts, the element can be
    // detached from the document for a tick before its callback ref clears the registry entry.
    // Feeding a detached node to the Popper throws an MUI "invalid anchorEl" warning, so we treat
    // it as absent and fall through to the centered card until a live anchor re-registers.
    const rawAnchor = step.anchorId ? getAnchor(step.anchorId) : null
    const anchorEl = rawAnchor?.isConnected ? rawAnchor : null

    // Anchored card.
    if (anchorEl) {
        return (
            <Popper
                open
                anchorEl={anchorEl}
                placement={step.placement}
                sx={{ zIndex: ZIndex, pointerEvents: "none" }}
                modifiers={[
                    { name: "offset", options: { offset: [0, 12] } },
                    { name: "flip", enabled: false },
                    { name: "preventOverflow", options: { padding: 8 } },
                    { name: "arrow", enabled: true, options: { element: arrowRef, padding: 12 } },
                ]}
            >
                <TourCard
                    step={step}
                    stepIndex={stepIndex}
                    total={TOUR_STEPS.length}
                    onNext={next}
                    onPrev={prev}
                    onSkip={skip}
                    setArrowRef={setArrowRef}
                    arrowEdge={arrowEdgeFor(step.placement)}
                    nextDisabled={nextDisabled}
                />
            </Popper>
        )
    }

    // Centered fallback: anchorless steps, or an anchor that has not mounted yet.
    return (
        <Box
            sx={{
                position: "fixed",
                top: "50%",
                left: "50%",
                transform: "translate(-50%, -50%)",
                zIndex: ZIndex,
                pointerEvents: "none",
            }}
        >
            <TourCard
                step={step}
                stepIndex={stepIndex}
                total={TOUR_STEPS.length}
                onNext={next}
                onPrev={prev}
                onSkip={skip}
                nextDisabled={nextDisabled}
            />
        </Box>
    )
}

export default TourOverlay
