import { fireEvent, render, screen, waitFor } from "@testing-library/react"
import { describe, expect, test, vi } from "vitest"
import SplitButtonDropdown from "@/ui/components/SplitButtonDropdown"

const openMenu = () => fireEvent.click(screen.getByLabelText("Open dropdown"))

describe("SplitButtonDropdown", () => {
    test("selecting a row runs its action and closes the menu", async () => {
        const onSelect = vi.fn()
        render(
            <SplitButtonDropdown
                icon={<span />}
                onIconClick={vi.fn()}
                items={[{ key: "only", label: "Always shown", onSelect }]}
            />
        )

        openMenu()
        fireEvent.click(screen.getByRole("menuitem"))

        expect(onSelect).toHaveBeenCalledTimes(1)
        await waitFor(() => expect(screen.queryByRole("menu")).toBeNull())
    })

    test("the icon half runs its own action without opening the menu", () => {
        const onIconClick = vi.fn()
        render(
            <SplitButtonDropdown
                icon={<span data-testid="main-action" />}
                onIconClick={onIconClick}
                items={[{ key: "only", label: "Always shown", onSelect: vi.fn() }]}
            />
        )

        fireEvent.click(screen.getByTestId("main-action"))

        expect(onIconClick).toHaveBeenCalledTimes(1)
        expect(screen.queryByRole("menu")).toBeNull()
    })
})
