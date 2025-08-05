import { describe, expect, test, vi } from "vitest"
import Lazy from "@/util/Lazy"

describe("Lazy<T> Tests", () => {
    test("Should Call The Init Function Only Once", () => {
        const initFn = vi.fn(() => "computed value")
        const lazy = new Lazy(initFn)

        const val1 = lazy.getValue()
        const val2 = lazy.getValue()

        expect(val1).toBe("computed value")
        expect(val2).toBe("computed value")
        expect(initFn).toHaveBeenCalledTimes(1)
    })

    test("Should Not Initialize Until GetValue Is Called", () => {
        const initFn = vi.fn(() => "late init")
        const lazy = new Lazy(initFn)

        expect(initFn).not.toHaveBeenCalled()
        lazy.getValue()
        expect(initFn).toHaveBeenCalledTimes(1)
    })

    test("Should Work With Complex Types", () => {
        const initFn = () => ({ x: 1, y: 2 })
        const lazy = new Lazy(initFn)

        const val = lazy.getValue()
        expect(val).toEqual({ x: 1, y: 2 })
    })
})
