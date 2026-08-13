import type React from "react"
import { createContext, useContext } from "react"

export const FIT_TOLERANCE_PX = 8

export function computeVisibleCount(budget: number, widths: readonly number[], gap: number): number {
    const spendable = budget - FIT_TOLERANCE_PX

    let used = 0
    for (let i = 0; i < widths.length; i++) {
        used += widths[i] + gap
        if (used > spendable) return i
    }
    return widths.length
}

export interface TopBarFitValue {
    rowRef: React.RefObject<HTMLElement | null>
    spacerRef: React.RefObject<HTMLElement | null>
    // not read. increased on resize to give context value new identity & re-run
    resizeTick: number
}

export const TopBarFitContext = createContext<TopBarFitValue | undefined>(undefined)

export function useTopBarFit(): TopBarFitValue | undefined {
    return useContext(TopBarFitContext)
}
