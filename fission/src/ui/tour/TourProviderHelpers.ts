import { createContext, useContext } from "react"
import type { TourAnchorId } from "./tourSteps"

export interface TourContextValue {
    /** Whether the tour is currently running. */
    active: boolean
    /** Index into `TOUR_STEPS` of the current step. */
    stepIndex: number
    /** Advance to the next step (or finish on the last step). */
    next: () => void
    /** Go back a step (no-op on the first step). */
    prev: () => void
    /** Dismiss the tour and mark it as seen. */
    skip: () => void
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
    prev: noop,
    skip: noop,
    registerAnchor: noop,
    getAnchor: () => null,
    anchorVersion: 0,
})

export const useTourContext = () => useContext(TourContext)
