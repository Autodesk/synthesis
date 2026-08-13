import { Box, Popper } from "@mui/material"
import type { Instance as PopperInstance } from "@popperjs/core"
import type React from "react"
import { useEffect, useRef, useState } from "react"
import { TOP_BAR_HEIGHT } from "@/ui/components/topbar/TopBarConfig"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { ScreenPosition } from "./tourSteps"
import { TOUR_STEPS } from "./tourSteps"
import TourCard from "./TourCard"
import { useTourContext } from "./TourProviderHelpers"

const ZIndex = 1400 // above panels/modals (1300) and the top bar (1200)
const ScrimZIndex = ZIndex - 10
const SCRIM_COLOR = "rgba(0,0,0,0.5)"
// Gap from the top bar / viewport edge for an anchorless card that is pinned to a corner.
const SCREEN_EDGE_GAP = 12
const SPOTLIGHT_PAD = 6
const SETTLE_DELAYS = [0, 100, 250, 450]

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

const sameRect = (a: DOMRect | null, b: DOMRect) =>
    a !== null && a.top === b.top && a.left === b.left && a.width === b.width && a.height === b.height

function useAnchorRect(el: HTMLElement | null, enabled: boolean) {
    const [rect, setRect] = useState<DOMRect | null>(null)

    useEffect(() => {
        if (!el || !enabled) {
            setRect(null)
            return
        }
        const update = () =>
            setRect(prev => {
                const next = el.getBoundingClientRect()
                return sameRect(prev, next) ? prev : next
            })

        const timers = SETTLE_DELAYS.map(delay => setTimeout(update, delay))
        const observer = new ResizeObserver(update)
        observer.observe(el)
        window.addEventListener("resize", update)
        return () => {
            timers.forEach(clearTimeout)
            observer.disconnect()
            window.removeEventListener("resize", update)
        }
    }, [el, enabled])

    return rect
}

const SpotlightScrim: React.FC<{ rect: DOMRect }> = ({ rect }) => {
    const top = Math.max(0, rect.top - SPOTLIGHT_PAD)
    const left = Math.max(0, rect.left - SPOTLIGHT_PAD)
    const right = rect.right + SPOTLIGHT_PAD
    const bottom = rect.bottom + SPOTLIGHT_PAD

    const bands = {
        above: { top: 0, left: 0, right: 0, height: top },
        below: { top: bottom, left: 0, right: 0, bottom: 0 },
        before: { top, left: 0, width: left, height: bottom - top },
        after: { top, left: right, right: 0, height: bottom - top },
    }

    return (
        <>
            {Object.entries(bands).map(([edge, band]) => (
                <Box
                    key={edge}
                    sx={{
                        position: "fixed",
                        bgcolor: SCRIM_COLOR,
                        zIndex: ScrimZIndex,
                        pointerEvents: "auto",
                        ...band,
                    }}
                />
            ))}
            <Box
                sx={{
                    position: "fixed",
                    top,
                    left,
                    width: right - left,
                    height: bottom - top,
                    borderRadius: "6px",
                    boxShadow: "0 0 0 2px rgba(255,255,255,0.35)",
                    zIndex: ScrimZIndex,
                    pointerEvents: "none",
                }}
            />
        </>
    )
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
    const { active, stepIndex, canAdvance, next, prev, skip, nudge, getAnchor, anchorVersion } = useTourContext()
    const { blockState } = useUIContext()
    const [arrowRef, setArrowRef] = useState<HTMLElement | null>(null)
    const popperRef = useRef<PopperInstance>(null)

    const step = active ? TOUR_STEPS[stepIndex] : undefined

    // Resolved on every render; the context change from anchor (de)registration drives re-renders.
    // Guard on `isConnected`: while an anchor's host (a panel/modal) unmounts, the element can be
    // detached from the document for a tick before its callback ref clears the registry entry.
    // Feeding a detached node to the Popper throws an MUI "invalid anchorEl" warning, so we treat
    // it as absent and fall through to the centered card until a live anchor re-registers.
    const rawAnchor = step?.anchorId ? getAnchor(step.anchorId) : null
    const anchorEl = rawAnchor?.isConnected ? rawAnchor : null

    const spotlightRect = useAnchorRect(anchorEl, step?.focus === "anchor")

    useEffect(() => {
        const timers = SETTLE_DELAYS.map(delay => setTimeout(() => popperRef.current?.update(), delay))
        return () => timers.forEach(clearTimeout)
    }, [stepIndex, anchorVersion])

    if (!step) return null

    const nextDisabled = !canAdvance

    const scrim = blockState.blocked ? null : step.focus === "screen" ? (
        <Box sx={{ position: "fixed", inset: 0, bgcolor: SCRIM_COLOR, zIndex: ScrimZIndex, pointerEvents: "auto" }} />
    ) : spotlightRect ? (
        <SpotlightScrim rect={spotlightRect} />
    ) : null

    const card = (
        <TourCard
            step={step}
            stepIndex={stepIndex}
            total={TOUR_STEPS.length}
            onNext={nextDisabled ? nudge : next}
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
