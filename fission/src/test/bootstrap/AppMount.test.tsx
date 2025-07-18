import { afterAll, assert, describe, expect, beforeEach, expectTypeOf, test, vi } from "vitest"
import { server } from "@vitest/browser/context"

import { cleanup, render, RenderResult } from "vitest-browser-react"
import { ReactElement } from "react"
import World from "@/systems/World.ts"
const { readFile } = server.commands



let screen: RenderResult | null
const renderMock = vi.fn((children: ReactElement) => {
    screen = render(children)
})
vi.mock("react-dom/client", () => ({
    createRoot: vi.fn(() => ({ render: renderMock })),
}))

describe("React Mounting", async () => {
    beforeEach(async () => {
        document.documentElement.innerHTML = await readFile("index.html")
        screen = null
        vi.resetAllMocks()
    })

    afterAll(() => {
        cleanup()
        vi.resetModules()
    })

    test("Root element exists", () => {
        expect(document.getElementById("root")).not.toBeNull()
    })

    // importing main.tsx has side effects that I could not clean up and can only be done once (per file), so I am using one test and many annotations
    test("App fully mounts", async ({annotate}) => {
        await import("@/main.tsx")

        expect(window.convertAuthToken).toBeDefined()
        expectTypeOf(window.convertAuthToken).toBeFunction()
        expect(window.gtag).toBeDefined()
        expectTypeOf(window.gtag).toBeFunction()
        await annotate("expected global functions mount")

        expect(renderMock).toHaveBeenCalledOnce()
        assert(screen != null, "Screen was null")


        const screenElement = screen.baseElement
        expect(screenElement.querySelector("canvas")).toBeInTheDocument()
        expect(screen.getByText("Singleplayer")).toBeInTheDocument()
        await annotate("DOM successfully updated to include Synthesis components")

        const initWorldSpy = vi.spyOn(World, "initWorld")
        await screen.getByText("Singleplayer").click()
        expect(initWorldSpy).toHaveBeenCalledOnce()
        await annotate("Singleplayer Button calls initWorld")
        await annotate("Post-load DOM", {contentType:"text/html", body: document.documentElement.outerHTML})
    })
})
