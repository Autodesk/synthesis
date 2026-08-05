import { render, waitFor } from "@testing-library/react"
import type React from "react"
import { useRef } from "react"
import { describe, expect, test } from "vitest"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { TopBarFitProvider } from "@/ui/components/topbar/TopBarFitProvider"

const ITEM_COUNT = 5

const ITEMS: readonly CollapsibleItem[] = Array.from({ length: ITEM_COUNT }, (_, i) => ({
    key: `item-${i}`,
    node: <div data-testid="item" style={{ width: 40, height: 20 }} />,
}))

const Harness: React.FC<{ width: number }> = ({ width }) => {
    const rowRef = useRef<HTMLDivElement>(null)
    const spacerRef = useRef<HTMLDivElement>(null)

    return (
        <div ref={rowRef} style={{ display: "flex", alignItems: "center", gap: 12, width }}>
            <TopBarFitProvider rowRef={rowRef} spacerRef={spacerRef}>
                <CollapsibleGroup
                    items={ITEMS}
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
    })

    test("keeps the always slot even when every item has been shed", async () => {
        const { container } = render(<Harness width={80} />)

        await waitFor(() => expect(visibleItems(container)).toBe(0))
        expect(container.querySelectorAll("[data-testid=always]")).toHaveLength(1)
        expectNoOverflow(container)
    })
})
