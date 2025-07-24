import { install } from "@haensl/google-analytics"
import { HttpResponse, http } from "msw"
import { setupWorker } from "msw/browser"
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, Mock, test, vi } from "vitest"
import AnalyticsSystem from "@/systems/analytics/AnalyticsSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"

type RequestType = Parameters<Parameters<typeof http.get>[1]>[0]

describe("Analytics", () => {
    const gtagRequestMock: Mock<(req: RequestType) => void> = vi.fn(() => {})

    const restHandlers = [
        http.post("https://www.google-analytics.com/g/collect", req => {
            gtagRequestMock(req)
            return HttpResponse.text("")
        }),
        // http.all("*", _req => {
        //     return HttpResponse.text("")
        // }),
    ]

    const server = setupWorker(...restHandlers)

    // Start server before all tests
    beforeAll(async () => await server.start({ onUnhandledRequest: "bypass", quiet: true }))

    //  Close server after all tests
    afterAll(() => server.stop())

    // Reset handlers after each test `important for test isolation`
    afterEach(() => server.resetHandlers())
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

    describe("With Load", () => {
        beforeAll(async () => {
            vi.useFakeTimers()
            const script = document.createElement("script")
            script.src = "https://www.googletagmanager.com/gtag/js?id=G-6XNCRD7QNC"
            document.head.appendChild(script)
            await vi.waitUntil(() => window.dataLayer != null, { timeout: 3000 })
            install() // gtag is a function defined here to push to the datalayer object
        })

        test("google analytics loaded", async () => {
            expect(window.gtag).toBeDefined()
            expect(window.dataLayer).toBeDefined()
        })

        test("gtag calls fetch with appropriate values", async () => {
            PreferencesSystem.setGlobalPreference("ReportAnalytics", true)

            const initialParams = mockRequestParametersHandle()

            const gtagSpy = vi.spyOn(window, "gtag")
            const system = new AnalyticsSystem()
            expect(gtagSpy).toHaveBeenCalled()
            await initialParams.then(params => {
                expect(params.get("tid")).toBe("G-6XNCRD7QNC")
            })
            gtagSpy.mockClear()

            const eventParams = mockRequestParametersHandle()
            system.event("APS Calls per Minute", {})

            expect(gtagSpy).toHaveBeenCalled()
            await eventParams.then(params => {
                expect(params.get("tid")).toBe("G-6XNCRD7QNC")
                expect(params.get("en")).toBe("APS Calls per Minute")
            })
        }, 20000)
    })

    test("Calls appropriate gtag functions", async () => {
        const system = new AnalyticsSystem()
        const gtagMock = vi.spyOn(window, "gtag")
        const originalDataLayerSize = window.dataLayer!.length
        system.event("Cache Get", { key: "1234" })
        expect(gtagMock).toHaveBeenCalledExactlyOnceWith("event", "Cache Get", { key: "1234" })
        expect(window.dataLayer!.length).toBe(originalDataLayerSize + 1)
        expect(window.dataLayer![window.dataLayer!.length - 1]).toBe(["event", "Cache Get", { key: "1234" }])
    })
})
