import { fireEvent, render, screen, waitFor } from "@testing-library/react"
import { act } from "react"
import { describe, expect, test, vi } from "vitest"
import EventSystem from "@/systems/EventSystem"
import { DragModeButton } from "@/ui/components/TopBar"

const button = () => screen.getByRole("button")

describe("TopBar drag mode", () => {
    test("mirrors the drag mode command in both directions", async () => {
        const listener = vi.fn()
        const unlisten = EventSystem.listen("SetDragModeEvent", listener)

        render(<DragModeButton />)
        expect(button()).toHaveAttribute("aria-pressed", "false")

        act(() => EventSystem.dispatch("SetDragModeEvent", { enabled: true }))

        await waitFor(() => expect(button()).toHaveAttribute("aria-pressed", "true"))

        fireEvent.click(button())

        expect(listener).toHaveBeenLastCalledWith(expect.objectContaining({ enabled: false }))
        await waitFor(() => expect(button()).toHaveAttribute("aria-pressed", "false"))

        unlisten()
    })
})
