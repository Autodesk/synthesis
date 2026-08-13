import { createContext, useCallback, useContext } from "react"
import type { TourAnchorId } from "./TourSteps"

export interface TourContextValue {
    /** Whether the tour is currently running. */
    active: boolean
    /** Index into `TOUR_STEPS` of the current step. */
    stepIndex: number
    /** Advance to the next step (or finish on the last step). */
    next: () => void
    canAdvance: boolean
    /** Go back a step (no-op on the first step). */
    prev: () => void
    /** Dismiss the tour and mark it as seen. */
    skip: () => void
    nudge: () => void
    /** Register (or clear, with `null`) a DOM element as a named tour anchor. */
    registerAnchor: (id: TourAnchorId, el: HTMLElement | null) => void
    /** Resolve the current element registered for an anchor id. */
    getAnchor: (id: TourAnchorId) => HTMLElement | null
    /** Bumped whenever the anchor registry changes, so the overlay re-resolves. */
    anchorVersion: number
}

const noop = () => {}

export const TourContext = createContext<TourContextValue>({
    active: false,
    stepIndex: 0,
    next: noop,
    canAdvance: true,
    prev: noop,
    skip: noop,
    nudge: noop,
    registerAnchor: noop,
    getAnchor: () => null,
    anchorVersion: 0,
})

export const useTourContext = () => useContext(TourContext)

/**
 * Returns a callback ref that registers the attached DOM element as the named tour
 * anchor for the lifetime it is mounted. Attach it to the element a tour step points at:
 *
 * ```tsx
 * <IconButton ref={useTourAnchor("add-assembly")} ... />
 * ```
 *
 * The registered element is what the tour card's Popper anchors to. When the tour is
 * inactive this is effectively free - it just keeps the registry up to date.
 */
export function useTourAnchor(id: TourAnchorId) {
    const { registerAnchor } = useTourContext()
    return useCallback((el: HTMLElement | null) => registerAnchor(id, el), [registerAnchor, id])
}
