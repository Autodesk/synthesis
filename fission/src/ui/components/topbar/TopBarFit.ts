import type React from "react"
import { createContext, useContext } from "react"

export const TOP_BAR_FIT_SLACK = 8

export function computeVisibleCount(budget: number, widths: readonly number[], gap: number): number {
    let used = 0
    for (let i = 0; i < widths.length; i++) {
        used += widths[i] + gap
        if (used > budget) return i
    }
    return widths.length
}

export interface TopBarFitValue {
    spacerRef: React.RefObject<HTMLElement | null>
    resizeTick: number
}

export const TopBarFitContext = createContext<TopBarFitValue | undefined>(undefined)

export function useTopBarFit(): TopBarFitValue | undefined {
    return useContext(TopBarFitContext)
}
