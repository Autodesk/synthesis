import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import Joystick from "@/systems/scene/Joystick"
import { MAX_JOYSTICK_RADIUS } from "@/ui/components/TouchControls"

describe("Joystick Tests", () => {
    let joystick: Joystick
    let mockBaseElement: HTMLElement
    let mockStickElement: HTMLElement
    let mockBoundingRect: DOMRect

    beforeEach(() => {
        mockBaseElement = document.createElement("div")
        mockStickElement = document.createElement("div")

        mockBoundingRect = {
            left: 100,
            top: 100,
            right: 200,
            bottom: 200,
            width: 100,
            height: 100,
            x: 100,
            y: 100,
            toJSON: () => ({}),
        }

        vi.spyOn(mockBaseElement, "getBoundingClientRect").mockReturnValue(mockBoundingRect)

        vi.spyOn(mockBaseElement, "addEventListener")
        vi.spyOn(document, "addEventListener")

        joystick = new Joystick(mockBaseElement, mockStickElement)
    })

    afterEach(() => {
        vi.clearAllMocks()
        vi.restoreAllMocks()
    })

    describe("Pointer Events", () => {
        test("Should handle pointerdown event", () => {
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2,
                clientY: 150 - MAX_JOYSTICK_RADIUS / 2,
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(pointerDownEvent)

            expect(joystick.x).toBe(0.5)
            expect(joystick.y).toBe(-0.5)
        })

        test("Should handle pointermove event", () => {
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150,
                clientY: 150,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            const pointerMoveEvent = new PointerEvent("pointermove", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2,
                clientY: 150 + MAX_JOYSTICK_RADIUS / 2,
                pointerId: 1,
            })
            document.dispatchEvent(pointerMoveEvent)

            expect(joystick.x).toBe(0.5)
            expect(joystick.y).toBe(0.5)
        })

        test("Should ignore pointermove event from different pointer", () => {
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150,
                clientY: 150,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            const pointerMoveEvent = new PointerEvent("pointermove", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2,
                clientY: 150 + MAX_JOYSTICK_RADIUS / 2,
                pointerId: 2,
            })
            document.dispatchEvent(pointerMoveEvent)

            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
        })

        test("Should handle pointerup event and reset position", () => {
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2,
                clientY: 150 + MAX_JOYSTICK_RADIUS / 2,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            expect(joystick.x).toBe(0.5)
            expect(joystick.y).toBe(0.5)

            const pointerUpEvent = new PointerEvent("pointerup", {
                pointerId: 1,
            })
            document.dispatchEvent(pointerUpEvent)

            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
        })
    })

    describe("Edge Cases", () => {
        test("Should constrain position within max radius", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS * 2,
                clientY: 150 - MAX_JOYSTICK_RADIUS * 2,
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(joystick.x).toBeCloseTo(0.7, 1)
            expect(joystick.y).toBeCloseTo(-0.7, 1)
        })
    })
})
