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

    test("Static stylesheet links exist", () => {
        const links = [...document.querySelectorAll<HTMLLinkElement>('link[rel="stylesheet"]')]
        expect(links.map(link => link.href)).toEqual(
            expect.arrayContaining([
                expect.stringContaining("artifakt.css"),
                expect.stringContaining("fonts.googleapis.com"),
            ])
        )
    })

    // importing main.tsx has side effects that I could not clean up and can only be done once (per file),
    // so I am using one test and many annotations. It's possible that there's a better way, but I couldn't
    // find it in 4 hours of trying
    test("App fully mounts through main.tsx", async ({ annotate, skip }) => {
        skip(server.browser == "firefox", "WebGL bug in Github Actions on Firefox")

        const initWorldSpy = vi.spyOn(World, "initWorld")

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
        // The top bar renders icons as inline SVGs exposed through their accessible label.
        // On mobile view (smaller viewport), a hamburger menu ("Open menu") is rendered instead.
        const settingsIcon = screenElement.querySelector('[role="img"][aria-label="settings"]')
        const mobileMenu = screenElement.querySelector('[aria-label="Open menu"]')
        expect(settingsIcon || mobileMenu).toBeInTheDocument()
        await annotate("DOM successfully updated to include Synthesis components and the top bar")

        // No Singleplayer button anymore — the world initializes on mount.
        expect(initWorldSpy).toHaveBeenCalled()
        await annotate("World initialized automatically on mount")

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
