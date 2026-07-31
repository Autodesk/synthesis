import { Box, Stack } from "@mui/material"
import type React from "react"
import { Fragment, useLayoutEffect, useRef, useState } from "react"
import { TOP_BAR_GAP, TOP_BAR_GAP_PX } from "@/ui/components/topbar/TopBarConfig"
import { computeVisibleCount, TOP_BAR_FIT_SLACK, useTopBarFit } from "@/ui/components/topbar/TopBarFit"

export interface CollapsibleItem {
    key: string
    node: React.ReactNode
}

interface CollapsibleGroupProps {
    items: CollapsibleItem[]
    always?: React.ReactNode
}

const MEASURE_LAYER_SX = {
    position: "absolute",
    top: 0,
    left: 0,
    width: 0,
    height: 0,
    overflow: "hidden",
    visibility: "hidden",
    pointerEvents: "none",
} as const

const MEASURE_ROW_SX = { display: "flex", width: "max-content" } as const

const CollapsibleGroup: React.FC<CollapsibleGroupProps> = ({ items, always }) => {
    const fit = useTopBarFit()
    const measureRef = useRef<HTMLDivElement>(null)

    const [visibleCount, setVisibleCount] = useState(fit ? 0 : items.length)

    const itemsKey = items.map(item => item.key).join(" ")

    // biome-ignore lint/correctness/useExhaustiveDependencies: deliberate re-measure triggers
    useLayoutEffect(() => {
        const spacer = fit?.spacerRef.current
        const measure = measureRef.current
        if (!spacer || !measure) return

        const widths = Array.from(measure.children, child => (child as HTMLElement).offsetWidth)

        const usedByItems = widths.slice(0, visibleCount).reduce((total, width) => total + width + TOP_BAR_GAP_PX, 0)
        const room = spacer.offsetWidth + usedByItems

        setVisibleCount(computeVisibleCount(room - TOP_BAR_FIT_SLACK, widths, TOP_BAR_GAP_PX))
    }, [fit?.resizeTick, fit?.spacerRef, itemsKey, visibleCount])

    return (
        <Stack direction="row" alignItems="center" gap={TOP_BAR_GAP} sx={{ position: "relative", flexShrink: 0 }}>
            {items.slice(0, visibleCount).map(item => (
                <Fragment key={item.key}>{item.node}</Fragment>
            ))}

            {always}

            <Box aria-hidden sx={MEASURE_LAYER_SX}>
                <Box ref={measureRef} sx={MEASURE_ROW_SX}>
                    {items.map(item => (
                        <Fragment key={item.key}>{item.node}</Fragment>
                    ))}
                </Box>
            </Box>
        </Stack>
    )
}

export default CollapsibleGroup
