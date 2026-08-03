import { render, waitFor } from "@testing-library/react"
import type React from "react"
import { useEffect, useRef } from "react"
import { describe, expect, test } from "vitest"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { TopBarFitProvider } from "@/ui/components/topbar/TopBarFitProvider"

const ITEM_COUNT = 5
const ITEM_WIDTH = 40
const ALWAYS_WIDTH = 60

const item = (key: string, onMount?: () => void): CollapsibleItem => ({
    key,
    node: <Item onMount={onMount} />,
})

const Item: React.FC<{ onMount?: () => void }> = ({ onMount }) => {
    useEffect(() => {
        onMount?.()
    }, [onMount])

    return <div data-testid="item" style={{ width: ITEM_WIDTH, height: 20 }} />
}

const ITEMS = Array.from({ length: ITEM_COUNT }, (_, i) => item(`item-${i}`))

const Harness: React.FC<{ width: number; items?: readonly CollapsibleItem[] }> = ({ width, items = ITEMS }) => {
    const rowRef = useRef<HTMLDivElement>(null)
    const spacerRef = useRef<HTMLDivElement>(null)

    return (
        <div ref={rowRef} style={{ display: "flex", alignItems: "center", gap: 12, width }}>
            <TopBarFitProvider rowRef={rowRef} spacerRef={spacerRef}>
                <CollapsibleGroup
                    items={items}
                    always={<div data-testid="always" style={{ width: ALWAYS_WIDTH, height: 20, flexShrink: 0 }} />}
                />
                <div ref={spacerRef} style={{ flexGrow: 1 }} />
            </TopBarFitProvider>
        </div>
    )
}

const visibleItems = (container: HTMLElement) => container.querySelectorAll("[data-testid=item]").length

describe("CollapsibleGroup", () => {
    test("shows every item when the row has room to spare", async () => {
        const { container } = render(<Harness width={800} />)

        await waitFor(() => expect(visibleItems(container)).toBe(ITEM_COUNT))
    })

    test("sheds items when the row is too narrow, and never the always slot", async () => {
        const { container } = render(<Harness width={200} />)

        await waitFor(() => expect(visibleItems(container)).toBeLessThan(ITEM_COUNT))
        expect(visibleItems(container)).toBeGreaterThan(0)
        expect(container.querySelectorAll("[data-testid=always]")).toHaveLength(1)
    })

    test("sheds every item before it lets the always slot overflow", async () => {
        const { container } = render(<Harness width={80} />)

        await waitFor(() => expect(visibleItems(container)).toBe(0))
        expect(container.querySelectorAll("[data-testid=always]")).toHaveLength(1)
    })

    test("keeps the row from overflowing at any width", async () => {
        const { container, rerender } = render(<Harness width={800} />)
        const row = container.firstElementChild as HTMLElement

        for (const width of [800, 500, 340, 260, 180, 120]) {
            rerender(<Harness width={width} />)
            await waitFor(() => expect(row.scrollWidth).toBeLessThanOrEqual(row.clientWidth))
        }
    })

    test("brings items back as the row grows again", async () => {
        const { container, rerender } = render(<Harness width={200} />)

        await waitFor(() => expect(visibleItems(container)).toBeLessThan(ITEM_COUNT))

        rerender(<Harness width={800} />)

        await waitFor(() => expect(visibleItems(container)).toBe(ITEM_COUNT))
    })

    test("re-measures when the item set changes", async () => {
        const { container, rerender } = render(<Harness width={800} />)

        await waitFor(() => expect(visibleItems(container)).toBe(ITEM_COUNT))

        const fewer = ITEMS.slice(0, 2)
        rerender(<Harness width={800} items={fewer} />)

        await waitFor(() => expect(visibleItems(container)).toBe(fewer.length))
    })

    test("mounts each item exactly once", async () => {
        const mounts = new Map<string, number>()
        const counted = Array.from({ length: ITEM_COUNT }, (_, i) =>
            item(`counted-${i}`, () => mounts.set(`counted-${i}`, (mounts.get(`counted-${i}`) ?? 0) + 1))
        )

        const { container } = render(<Harness width={800} items={counted} />)

        await waitFor(() => expect(visibleItems(container)).toBe(ITEM_COUNT))
        expect([...mounts.values()]).toEqual(Array<number>(ITEM_COUNT).fill(1))
    })
})
