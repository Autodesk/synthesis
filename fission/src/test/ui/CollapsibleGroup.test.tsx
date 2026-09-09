import { render, waitFor } from "@testing-library/react"
import type React from "react"
import { useRef } from "react"
import { describe, expect, test } from "vitest"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { TOP_BAR_GAP_PX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarFitProvider } from "@/ui/components/topbar/TopBarFitProvider"

const ITEM_COUNT = 5

const makeItems = (count: number, width: number): CollapsibleItem[] =>
    Array.from({ length: count }, (_, i) => ({
        key: `w${width}-${i}`,
        node: <div data-testid="item" style={{ width, height: 20 }} />,
    }))

const ITEMS = makeItems(ITEM_COUNT, 40)

const Harness: React.FC<{ width: number; items?: readonly CollapsibleItem[] }> = ({ width, items = ITEMS }) => {
    const rowRef = useRef<HTMLDivElement>(null)
    const spacerRef = useRef<HTMLDivElement>(null)

    return (
        <div ref={rowRef} style={{ display: "flex", alignItems: "center", gap: TOP_BAR_GAP_PX, width }}>
            <TopBarFitProvider rowRef={rowRef} spacerRef={spacerRef}>
                <CollapsibleGroup
                    items={items}
                    always={<div data-testid="always" style={{ width: 60, height: 20, flexShrink: 0 }} />}
                />
                <div ref={spacerRef} style={{ flexGrow: 1 }} />
            </TopBarFitProvider>
        </div>
    )
}

const visibleItems = (container: HTMLElement) => container.querySelectorAll("[data-testid=item]").length

const expectNoOverflow = (container: HTMLElement) => {
    const row = container.firstElementChild as HTMLElement
    expect(row.scrollWidth).toBeLessThanOrEqual(row.clientWidth)
}

describe("CollapsibleGroup", () => {
    test("sheds items to fit the row, and brings them back as it grows again", async () => {
        const { container, rerender } = render(<Harness width={200} />)

        await waitFor(() => expect(visibleItems(container)).toBeLessThan(ITEM_COUNT))
        expect(visibleItems(container)).toBeGreaterThan(0)
        expectNoOverflow(container)

        rerender(<Harness width={800} />)

        await waitFor(() => expect(visibleItems(container)).toBe(ITEM_COUNT))
        expectNoOverflow(container)

        rerender(<Harness width={80} />)

        await waitFor(() => expect(visibleItems(container)).toBe(0))
        expect(container.querySelectorAll("[data-testid=always]")).toHaveLength(1)
        expectNoOverflow(container)
    })

    test("re-measures when the item set changes instead of reusing stale widths", async () => {
        const { container, rerender } = render(<Harness width={400} items={makeItems(5, 40)} />)

        await waitFor(() => expect(visibleItems(container)).toBe(5))

        rerender(<Harness width={400} items={makeItems(5, 150)} />)

        await waitFor(() => expect(visibleItems(container)).toBeLessThan(5))
        expectNoOverflow(container)
    })
})
