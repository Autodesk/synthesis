import { fireEvent, render, screen, waitFor, within } from "@testing-library/react"
import { act } from "react"
import { describe, expect, test, vi } from "vitest"
import EventSystem from "@/systems/EventSystem"
import TopBar from "@/ui/components/TopBar"

const dragButton = () => within(screen.getByLabelText(/Drag Mode$/)).getByRole("button")

describe("TopBar drag mode", () => {
    test("mirrors the drag mode command in both directions", async () => {
        const listener = vi.fn()
        const unlisten = EventSystem.listen("SetDragModeEvent", listener)

        render(<TopBar />)
        expect(dragButton()).toHaveAttribute("aria-pressed", "false")

        act(() => EventSystem.dispatch("SetDragModeEvent", { enabled: true }))

        await waitFor(() => expect(dragButton()).toHaveAttribute("aria-pressed", "true"))

        fireEvent.click(dragButton())

        expect(listener).toHaveBeenLastCalledWith(expect.objectContaining({ enabled: false }))
        await waitFor(() => expect(dragButton()).toHaveAttribute("aria-pressed", "false"))

        unlisten()
    })
})
