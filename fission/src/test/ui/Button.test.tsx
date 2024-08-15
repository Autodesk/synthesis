import { render, fireEvent, getByText } from "@testing-library/react"
import { assert, describe, expect, test } from "vitest"
import Button from "@/ui/components/Button"

describe("Button", () => {
    test("Click Enabled Button", () => {
        let buttonClicked = false
        const container = render(<Button onClick={() => (buttonClicked = true)} value="Test Button" />).container

        const button = getByText(container, "Test Button")
        assert(button != undefined)

        fireEvent.click(button)
        expect(buttonClicked).toBe(true)
    })

    test("Click Disabled Button", () => {
        let buttonClicked = false
        const container = render(
            <Button onClick={() => (buttonClicked = true)} value="Test Button" disabled={true} />
        ).container

        const button = getByText(container, "Test Button")
        assert(button != undefined)

        fireEvent.click(button)
        expect(buttonClicked).toBe(false)
    })
})
