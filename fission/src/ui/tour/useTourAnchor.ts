import { useCallback } from "react"
import { useTourContext } from "./TourProviderHelpers"
import type { TourAnchorId } from "./tourSteps"

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
