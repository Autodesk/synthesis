import { Box, Popper } from "@mui/material"
import type { Instance as PopperInstance } from "@popperjs/core"
import type React from "react"
import { useEffect, useRef, useState } from "react"
import { TOP_BAR_HEIGHT } from "@/ui/components/topbar/TopBarConfig"
import type { ScreenPosition } from "./tourSteps"
import { TOUR_STEPS } from "./tourSteps"
import TourCard from "./TourCard"
import { useTourContext } from "./TourProviderHelpers"

const ZIndex = 1400 // above panels/modals (1300) and the top bar (1200)
// Full-screen blocking scrim for informational steps: dims the app and swallows every click,
// so only the tour card (which sits above it at `ZIndex`) stays interactive.
const ScrimZIndex = ZIndex - 10
// Gap from the top bar / viewport edge for an anchorless card that is pinned to a corner.
const SCREEN_EDGE_GAP = 12

/** Fixed-position style for an anchorless card, keyed by its {@link ScreenPosition}. */
function screenPositionStyle(position: ScreenPosition | undefined) {
    if (position === "top-left") {
        return { top: TOP_BAR_HEIGHT + SCREEN_EDGE_GAP, left: SCREEN_EDGE_GAP }
    }
    // Default: dead center.
    return { top: "50%", left: "50%", transform: "translate(-50%, -50%)" }
}

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
    const { active, stepIndex, next, prev, skip, getAnchor, anchorVersion } = useTourContext()
    const [arrowRef, setArrowRef] = useState<HTMLElement | null>(null)
    const popperRef = useRef<PopperInstance>(null)

    const step = active ? TOUR_STEPS[stepIndex] : undefined

    // Re-sync the Popper position over a short burst after an anchored step (re)mounts. Some anchors
    // (e.g. the Assembly Setup panel) slide in via a CSS transform, which fires no resize/scroll
    // event, so the Popper's one-shot measurement lands on the anchor's pre-animation position and
    // never corrects. Nudging update() as the anchor settles fixes that; it is idempotent otherwise.
    useEffect(() => {
        const timers = [0, 100, 250, 450].map(delay => setTimeout(() => popperRef.current?.update(), delay))
        return () => timers.forEach(clearTimeout)
    }, [stepIndex, anchorVersion])

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

    // Informational steps float above a full-screen scrim that dims the app and absorbs every click
    // (no handler = clicks go nowhere), leaving only the card's buttons live. Rendered as a sibling
    // beneath the card so it never covers the card itself.
    const scrim = step.informational ? (
        <Box sx={{ position: "fixed", inset: 0, bgcolor: "rgba(0,0,0,0.5)", zIndex: ScrimZIndex, pointerEvents: "auto" }} />
    ) : null

    const card = (
        <TourCard
            step={step}
            stepIndex={stepIndex}
            total={TOUR_STEPS.length}
            onNext={next}
            onPrev={prev}
            onSkip={skip}
            nextDisabled={nextDisabled}
            {...(anchorEl && { setArrowRef, arrowEdge: arrowEdgeFor(step.placement) })}
        />
    )

    // Anchored card.
    if (anchorEl) {
        return (
            <>
                {scrim}
                <Popper
                    open
                    popperRef={popperRef}
                    anchorEl={anchorEl}
                    placement={step.placement}
                    sx={{ zIndex: ZIndex, pointerEvents: "none" }}
                    modifiers={[
                        { name: "offset", options: { offset: [0, 12] } },
                        { name: "flip", enabled: false },
                        // altAxis clamps along the placement axis itself (x for a "left" card) and
                        // tether:false lets it detach from an oversized reference, so the card stays
                        // fully on-screen instead of running off the edge - e.g. the near-full-screen
                        // Library modal, whose left edge would otherwise push the card off-viewport.
                        { name: "preventOverflow", options: { padding: 8, altAxis: true, tether: false } },
                        { name: "arrow", enabled: true, options: { element: arrowRef, padding: 12 } },
                    ]}
                >
                    {card}
                </Popper>
            </>
        )
    }

    // Anchorless steps (or an anchor that has not mounted yet): pin the card to a screen position.
    return (
        <>
            {scrim}
            <Box
                sx={{
                    position: "fixed",
                    ...screenPositionStyle(step.screenPosition),
                    zIndex: ZIndex,
                    pointerEvents: "none",
                }}
            >
                {card}
            </Box>
        </>
    )
}

export default TourOverlay
