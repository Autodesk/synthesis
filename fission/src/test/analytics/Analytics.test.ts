import { install } from "@haensl/google-analytics"
import { server } from "@vitest/browser/context"
import { HttpResponse, http } from "msw"
import { setupWorker } from "msw/browser"
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, expectTypeOf, type Mock, test, vi } from "vitest"
import AnalyticsSystem from "@/systems/analytics/AnalyticsSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"

type RequestType = Parameters<Parameters<typeof http.get>[1]>[0]
const tagID = "G-6XNCRD7QNC"

describe("Analytics", () => {
    const gtagRequestMock: Mock<(req: RequestType) => void> = vi.fn(() => {})

    const restHandlers = [
        http.post("https://www.google-analytics.com/g/collect", req => {
            gtagRequestMock(req)
            return HttpResponse.text("")
        }),
    ]

    const webMocks = setupWorker(...restHandlers)

    // Start server before all tests
    beforeAll(async () => await webMocks.start({ onUnhandledRequest: "bypass", quiet: true }))

    //  Close server after all tests
    afterAll(() => webMocks.stop())

    // Reset handlers after each test `important for test isolation`
    afterEach(() => webMocks.resetHandlers())
    beforeEach(() => {
        vi.resetAllMocks()
    })

    afterEach(() => {})

    const mockRequestParametersHandle = () => {
        return new Promise<URLSearchParams>(resolve => {
            gtagRequestMock.mockImplementationOnce(req => {
                resolve(new URL(req.request.url).searchParams)
            })
        })
    }

    describe("With gtag Script", () => {
        beforeAll(async () => {
            vi.useFakeTimers()
            const script = document.createElement("script")
            script.src = "https://www.googletagmanager.com/gtag/js?id=" + tagID
            document.head.appendChild(script)
            await vi.waitUntil(() => window.dataLayer != null, { timeout: 3000 })
            install() // gtag is a function defined here to push to the datalayer object
        })

        test("google analytics loaded", async () => {
            expect(window.gtag).toBeDefined()
            expect(window.dataLayer).toBeDefined()
            expectTypeOf(window.gtag!).toBeFunction()
            expectTypeOf(window.dataLayer!).toBeArray()
        })

        test("gtag calls fetch with appropriate values", async ({ skip }) => {
            skip(server.browser == "firefox", "Firefox blocks Google Analytics")
            PreferencesSystem.setGlobalPreference("ReportAnalytics", true)

            const initialParams = mockRequestParametersHandle()

            const gtagSpy: Mock<NonNullable<typeof window.gtag>> = vi.spyOn(window, "gtag")
            const system = new AnalyticsSystem()
            expect(gtagSpy).toHaveBeenCalled()
            await initialParams.then(params => {
                expect(params.get("tid")).toBe(tagID)
            })
            gtagSpy.mockClear()

            const eventParams = mockRequestParametersHandle()
            system.event("APS Calls per Minute", {})

            expect(gtagSpy).toHaveBeenCalled()
            await eventParams.then(params => {
                expect(params.get("tid")).toBe(tagID)
                expect(params.get("en")).toBe("APS Calls per Minute")
            })
        }, 20000)
    })

    describe("Without gtag Script", () => {
        beforeEach(() => {
            window.dataLayer = undefined
            window.gtag = undefined
            install()
        })

        test("gtag propagates to dataLayer", () => {
            const initialSize = window.dataLayer!.length
            window.gtag!("event", "test", { a: 2 })
            expect(window.dataLayer!.length).toBe(initialSize + 1)
            const lastDatalayerItem = window.dataLayer![window.dataLayer!.length - 1]
            expect(lastDatalayerItem[0]).toBe("event")
            expect(lastDatalayerItem[1]).toBe("test")
            expect(lastDatalayerItem[2]).toStrictEqual(expect.objectContaining({ a: 2 }))
        })
    })
})
