import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

describe("TopBarIcon", () => {
    test("renders inline svg markup without img tags", () => {
        const { container } = render(<TopBarIcon name="settings" />)

        expect(container.querySelector("svg")).toBeInTheDocument()
        expect(container.querySelector("img")).toBeNull()
    })

    test("clones icon markup for each instance", () => {
        const { container } = render(
            <>
                <TopBarIcon name="settings" />
                <TopBarIcon name="settings" />
            </>
        )

        expect(container.querySelectorAll("svg")).toHaveLength(2)
    })
})
