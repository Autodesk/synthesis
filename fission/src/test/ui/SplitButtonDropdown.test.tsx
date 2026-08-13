import { fireEvent, render, screen, waitFor } from "@testing-library/react"
import { describe, expect, test, vi } from "vitest"
import SplitButtonDropdown, { type SplitButtonMenuItem } from "@/ui/components/SplitButtonDropdown"

const openMenu = () => fireEvent.click(screen.getByLabelText("Open dropdown"))

describe("SplitButtonDropdown", () => {
    test("clicking a row's leading icon still selects the item and closes the menu", async () => {
        const onSelect = vi.fn()
        const items: SplitButtonMenuItem[] = [
            { key: "only", label: "Always shown", icon: <span data-testid="leading-icon" />, onSelect },
        ]
        render(<SplitButtonDropdown icon={<span>icon</span>} onIconClick={vi.fn()} items={items} />)

        openMenu()
        fireEvent.click(screen.getByTestId("leading-icon"))

        expect(onSelect).toHaveBeenCalledTimes(1)
        await waitFor(() => expect(screen.queryByRole("menu")).toBeNull())
    })

    test("the icon half runs its own action without opening the menu", () => {
        const onIconClick = vi.fn()
        const items: SplitButtonMenuItem[] = [{ key: "only", label: "Always shown", onSelect: vi.fn() }]
        render(<SplitButtonDropdown icon={<span>icon</span>} onIconClick={onIconClick} items={items} />)

        fireEvent.click(screen.getByText("icon"))

        expect(onIconClick).toHaveBeenCalledTimes(1)
        expect(screen.queryByRole("menu")).toBeNull()
    })

    test("reserves the icon slot on every row once any row has an icon", () => {
        const items: SplitButtonMenuItem[] = [
            { key: "with", label: "With icon", icon: <span data-testid="leading-icon" />, onSelect: vi.fn() },
            { key: "without", label: "Without icon", onSelect: vi.fn() },
        ]
        render(<SplitButtonDropdown icon={<span>icon</span>} onIconClick={vi.fn()} items={items} />)

        openMenu()

        const rows = screen.getAllByRole("menuitem")
        const slotWidth = (row: HTMLElement) => row.firstElementChild!.getBoundingClientRect().width

        expect(rows).toHaveLength(2)
        expect(slotWidth(rows[1])).toBe(slotWidth(rows[0]))
        expect(slotWidth(rows[1])).toBeGreaterThan(0)
    })
})
