import type React from "react"
import { useEffect, useMemo, useState } from "react"
import { TopBarFitContext } from "@/ui/components/topbar/TopBarFit"

interface TopBarFitProviderProps {
    rowRef: React.RefObject<HTMLElement | null>
    spacerRef: React.RefObject<HTMLElement | null>
    children: React.ReactNode
}

export const TopBarFitProvider: React.FC<TopBarFitProviderProps> = ({ rowRef, spacerRef, children }) => {
    const [resizeTick, setResizeTick] = useState(0)

    useEffect(() => {
        const row = rowRef.current
        if (!row) return

        const observer = new ResizeObserver(() => setResizeTick(tick => tick + 1))
        observer.observe(row)
        return () => observer.disconnect()
    }, [rowRef])

    const value = useMemo(() => ({ spacerRef, resizeTick }), [spacerRef, resizeTick])

    return <TopBarFitContext.Provider value={value}>{children}</TopBarFitContext.Provider>
}
