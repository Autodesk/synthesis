import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import { TOP_BAR_ICON_NAMES, TopBarIcon } from "@/ui/components/topbar/TopBarIcons"

describe("TopBarIcon", () => {
    test("every icon name maps to an svg on disk, and every svg on disk is named", () => {
        const onDisk = Object.keys(import.meta.glob("../../ui/components/topbar/icons/*.svg")).map(path =>
            path.replace(/^.*\//, "").replace(/\.svg$/, "")
        )

        expect([...TOP_BAR_ICON_NAMES].sort()).toEqual(onDisk.sort())

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
