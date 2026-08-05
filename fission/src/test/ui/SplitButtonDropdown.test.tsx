import { fireEvent, render, screen, waitFor } from "@testing-library/react"
import { describe, expect, test, vi } from "vitest"
import SplitButtonDropdown, { type SplitButtonMenuItem } from "@/ui/components/SplitButtonDropdown"

const renderDropdown = (items: SplitButtonMenuItem[], onIconClick = vi.fn()) => {
    render(<SplitButtonDropdown icon={<span>icon</span>} onIconClick={onIconClick} items={items} />)
    return onIconClick
}

const openMenu = () => fireEvent.click(screen.getByLabelText("Open dropdown"))

const expectMenuClosed = () => waitFor(() => expect(screen.queryByRole("menu")).toBeNull())

describe("SplitButtonDropdown", () => {
    test("a click anywhere inside a row selects it and closes the menu", async () => {
        const onSelect = vi.fn()
        renderDropdown([{ key: "only", label: "Always shown", icon: <span data-testid="leading-icon" />, onSelect }])

        openMenu()
        fireEvent.click(screen.getByTestId("leading-icon"))

        expect(onSelect).toHaveBeenCalledTimes(1)
        await expectMenuClosed()
    })

    test("the icon half runs its own action without opening the menu", () => {
        const onIconClick = renderDropdown([{ key: "only", label: "Always shown", onSelect: vi.fn() }])

        fireEvent.click(screen.getByText("icon"))

        expect(onIconClick).toHaveBeenCalledTimes(1)
        expect(screen.queryByRole("menu")).toBeNull()
    })
})
