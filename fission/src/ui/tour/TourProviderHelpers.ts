import { createContext, useCallback, useContext } from "react"
import type { TourAnchorId } from "./TourSteps"

export interface TourContextValue {
    active: boolean
    stepIndex: number
    next: () => void
    canAdvance: boolean
    prev: () => void
    skip: () => void
    registerAnchor: (id: TourAnchorId, element: HTMLElement | null) => void
    getAnchor: (id: TourAnchorId) => HTMLElement | null
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
    registerAnchor: noop,
    getAnchor: () => null,
    anchorVersion: 0,
})

export const useTourContext = () => useContext(TourContext)

// anchor tourcard to a component
//   <IconButton ref={useTourAnchor("add-assembly")} ... />
export function useTourAnchor(id: TourAnchorId) {
    const { registerAnchor } = useTourContext()
    return useCallback((element: HTMLElement | null) => registerAnchor(id, element), [registerAnchor, id])
}
