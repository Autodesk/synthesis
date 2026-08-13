import { Stack } from "@mui/material"
import type React from "react"
import { Fragment, useLayoutEffect, useRef, useState } from "react"
import { TOP_BAR_GAP, TOP_BAR_GAP_PX } from "@/ui/components/topbar/TopBarConfig"
import { computeVisibleCount, useTopBarFit } from "@/ui/components/topbar/TopBarFit"

export interface CollapsibleItem {
    key: string
    node: React.ReactNode
}

interface CollapsibleGroupProps {
    items: readonly CollapsibleItem[]
    always?: React.ReactNode
}

// '& > *' selects the direct children.
// the group nor buttons should be shrunk by flex row
const GROUP_SX = { flexShrink: 0, "& > *": { flexShrink: 0 } } as const

export const CollapsibleGroup: React.FC<CollapsibleGroupProps> = ({ items, always }) => {
    const fit = useTopBarFit()
    const groupRef = useRef<HTMLDivElement>(null)
    const widthsRef = useRef<readonly number[]>([])

    const itemCount = items.length
    const itemsKey = JSON.stringify(items.map(item => item.key))

    // adding guard to ensure 'fitted' isn't stale during renders
    const [fitted, setFitted] = useState({ itemsKey, visibleCount: itemCount })
    if (fitted.itemsKey !== itemsKey) {
        widthsRef.current = []
        setFitted({ itemsKey, visibleCount: itemCount })
    }
    const visibleCount = fitted.itemsKey === itemsKey ? fitted.visibleCount : itemCount

    useLayoutEffect(() => {
        const group = groupRef.current
        if (!group) return

        if (widthsRef.current.length !== itemCount) {
            if (group.children.length < itemCount) return

            widthsRef.current = Array.from(group.children)
                .slice(0, itemCount)
                .map(child => (child as HTMLElement).offsetWidth)
        }

        const row = fit?.rowRef.current
        const spacer = fit?.spacerRef.current
        if (!row || !spacer) return

        // width the items occupy; free space the flex spacer is taking rn; subtracting anything overflowing out of the row already
        const widths = widthsRef.current
        const usedByItems = widths.slice(0, visibleCount).reduce((total, width) => total + width + TOP_BAR_GAP_PX, 0)
        const overflow = Math.max(0, row.scrollWidth - row.clientWidth)
        const budget = usedByItems + spacer.offsetWidth - overflow

        const nextCount = computeVisibleCount(budget, widths, TOP_BAR_GAP_PX)
        setFitted(prev => (prev.visibleCount === nextCount ? prev : { itemsKey, visibleCount: nextCount }))
    }, [fit, itemCount, itemsKey, visibleCount])

    return (
        <Stack ref={groupRef} direction="row" alignItems="center" gap={TOP_BAR_GAP} sx={GROUP_SX}>
            {items.slice(0, visibleCount).map(item => (
                <Fragment key={item.key}>{item.node}</Fragment>
            ))}

            {always}
        </Stack>
    )
}
