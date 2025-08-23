import { server } from "@vitest/browser/context"
import type { ReactElement } from "react"
import { afterAll, assert, beforeEach, describe, expect, expectTypeOf, test, vi } from "vitest"
import { cleanup, type RenderResult, render } from "vitest-browser-react"
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

    test("Root element exists", async () => {
        expect(document.getElementById("root")).not.toBeNull()
    })

    test("Static stylesheets load", async () => {
        await vi.waitUntil(() => document.styleSheets.length >= 2, { timeout: 10000, interval: 200 })

        expect(document.styleSheets.length).toBe(2)
        const iterable = document.fonts.values()
        let iterator = iterable.next()
        let hasArtifaktFont = false
        while (!iterator.done) {
            if (iterator.value.family.includes("Artifakt")) {
                hasArtifaktFont = true
                break
            }
            iterator = iterable.next()
        }
        expect(hasArtifaktFont).toBeTruthy()
    })

    // importing main.tsx has side effects that I could not clean up and can only be done once (per file),
    // so I am using one test and many annotations. It's possible that there's a better way, but I couldn't
    // find it in 4 hours of trying
    test("App fully mounts through main.tsx", async ({ annotate, skip }) => {
        skip(server.browser == "firefox", "WebGL bug in Github Actions on Firefox")

        // biome-ignore lint/suspicious/noTsIgnore: ts-expect-error doesn't work here for some reason
        // @ts-ignore funky dynamic import
        await import("@/main.tsx")

        expect(window.convertAuthToken).toBeDefined()
        expectTypeOf(window.convertAuthToken).toBeFunction()
        expect(window.gtag).toBeDefined()
        expectTypeOf(window.gtag!).toBeFunction()
        await annotate("expected global functions mount")

        // assorted style rules from index.css
        const style = window.getComputedStyle(document.body)
        expect(style.overflow).toBe("hidden")
        expect(style.overscrollBehavior).toBe("none")
        expect(style.fontFamily.split(",")[0].trim()).toBe("Artifakt")
        await annotate("index.css applied correctly")

        expect(renderMock).toHaveBeenCalledOnce()
        assert(screen != null, "Screen was null")

        await wait(50)

        const screenElement = screen.baseElement
        expect(screenElement.querySelector("canvas")).toBeInTheDocument()
        expect(screen.getByText("Singleplayer")).toBeInTheDocument()
        await annotate("DOM successfully updated to include Synthesis components")
        const initWorldSpy = vi.spyOn(World, "initWorld")
        // for some reason threejs canvas intercepts .click()
        screen
            .getByText("Singleplayer")
            .element()
            .dispatchEvent(new PointerEvent("click", { bubbles: true }))
        expect(initWorldSpy).toHaveBeenCalledOnce()
        await annotate("Singleplayer Button calls initWorld")

        await wait(50)

        await annotate("Initial Scene DOM", { contentType: "text/html", body: document.documentElement.outerHTML })

        screen.unmount()

        await annotate("Screen unmounted gracefully")
    }, 20000)
})

function wait(milliseconds: number) {
    return new Promise(resolve => {
        setTimeout(resolve, milliseconds)
    })
}
