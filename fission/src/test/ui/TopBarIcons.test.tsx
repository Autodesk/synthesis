import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { TOP_BAR_ICON_NAMES, TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

describe("TopBarIcon", () => {
    test("every declared icon name resolves to an svg file", () => {
        for (const name of TOP_BAR_ICON_NAMES) {
            const { container } = render(<TopBarIcon name={name} />)

            expect(container.querySelector("svg"), `no svg rendered for "${name}"`).toBeInTheDocument()
        }
    })

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
