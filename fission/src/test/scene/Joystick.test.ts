import { test, expect, describe, beforeEach, afterEach, vi } from "vitest"
import Joystick from "@/systems/scene/Joystick"
import { MAX_JOYSTICK_RADIUS } from "@/ui/components/TouchControls"

describe("Joystick Tests", () => {
    let joystick: Joystick
    let mockBaseElement: HTMLElement
    let mockStickElement: HTMLElement
    let mockBoundingRect: DOMRect

    beforeEach(() => {
        // Create mock DOM elements
        mockBaseElement = document.createElement("div")
        mockStickElement = document.createElement("div")

        // Mock getBoundingClientRect to return predictable values
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

        // Mock addEventListener
        vi.spyOn(mockBaseElement, "addEventListener")
        vi.spyOn(document, "addEventListener")

        // Create joystick instance
        joystick = new Joystick(mockBaseElement, mockStickElement)
    })

    afterEach(() => {
        vi.clearAllMocks()
        vi.restoreAllMocks()
    })

    describe("Constructor and Initialization", () => {
        test("Should start with zero position", () => {
            expect(joystick).toBeDefined()
            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
            expect(joystick.getPosition("x")).toBe(0)
            expect(joystick.getPosition("y")).toBe(0)
        })
    })

    describe("Position Getters", () => {
        test("Should return normalized position values", () => {
            // Set internal position (simulating stick movement)
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150, // Center of base element (100 + 50)
                clientY: 150, // Center of base element (100 + 50)
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(joystick.x).toBe(0) // Should be 0 when at center
            expect(joystick.y).toBe(0) // Should be 0 when at center
        })

        test("Should return correct position for x axis", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2, // Half way to max radius
                clientY: 150, // Center Y
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(joystick.getPosition("x")).toBeCloseTo(0.5, 1)
            expect(joystick.getPosition("y")).toBeCloseTo(0, 1)
        })

        test("Should return correct position for y axis", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150, // Center X
                clientY: 150 + MAX_JOYSTICK_RADIUS / 2, // Half way to max radius
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(joystick.getPosition("x")).toBeCloseTo(0, 1)
            expect(joystick.getPosition("y")).toBeCloseTo(0.5, 1)
        })
    })

    describe("Pointer Events", () => {
        test("Should handle pointerdown event", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 175, // 25 pixels right of center
                clientY: 175, // 25 pixels down from center
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(mockBaseElement.getBoundingClientRect).toHaveBeenCalled()
            expect(joystick.x).toBeCloseTo(0.45, 1) // 25 / MAX_JOYSTICK_RADIUS
            expect(joystick.y).toBeCloseTo(0.45, 1) // 25 / MAX_JOYSTICK_RADIUS
        })

        test("Should handle pointermove event only for active pointer", () => {
            // First, start with pointerdown
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150,
                clientY: 150,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            // Then move with same pointer ID
            const pointerMoveEvent = new PointerEvent("pointermove", {
                clientX: 175,
                clientY: 175,
                pointerId: 1,
            })
            document.dispatchEvent(pointerMoveEvent)

            expect(joystick.x).toBeCloseTo(0.45, 1)
            expect(joystick.y).toBeCloseTo(0.45, 1)
        })

        test("Should ignore pointermove event from different pointer", () => {
            // Start with pointerdown
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 150,
                clientY: 150,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            // Move with different pointer ID
            const pointerMoveEvent = new PointerEvent("pointermove", {
                clientX: 175,
                clientY: 175,
                pointerId: 2, // Different pointer ID
            })
            document.dispatchEvent(pointerMoveEvent)

            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
        })

        test("Should handle pointerup event and reset position", () => {
            // Start with pointerdown
            const pointerDownEvent = new PointerEvent("pointerdown", {
                clientX: 175,
                clientY: 175,
                pointerId: 1,
            })
            mockBaseElement.dispatchEvent(pointerDownEvent)

            // Verify position is set
            expect(joystick.x).not.toBe(0)
            expect(joystick.y).not.toBe(0)

            // Then pointerup
            const pointerUpEvent = new PointerEvent("pointerup", {
                pointerId: 1,
            })
            document.dispatchEvent(pointerUpEvent)

            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
        })
    })

    describe("Position Constraints", () => {
        test("Should constrain position within max radius", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS * 2, // Way beyond max radius
                clientY: 150, // Center Y
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(Math.abs(joystick.x)).toBeLessThanOrEqual(1)
            expect(Math.abs(joystick.y)).toBeLessThanOrEqual(1)
        })

        test("Should maintain correct angle when constrained", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS * 2, // Way beyond max radius
                clientY: 150 + MAX_JOYSTICK_RADIUS * 2, // Way beyond max radius (45 degree angle)
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            // Should maintain 45 degree angle (equal x and y components)
            expect(Math.abs(joystick.x)).toBeCloseTo(Math.abs(joystick.y), 1)
            expect(joystick.x).toBeCloseTo(Math.sqrt(2) / 2, 1) // cos(45°)
            expect(joystick.y).toBeCloseTo(Math.sqrt(2) / 2, 1) // sin(45°)
        })

        test("Should not constrain position within max radius", () => {
            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150 + MAX_JOYSTICK_RADIUS / 2, // Half of max radius
                clientY: 150, // Center Y
                pointerId: 1,
            })

            mockBaseElement.dispatchEvent(mockPointerEvent)

            expect(joystick.x).toBeCloseTo(0.5, 1)
            expect(joystick.y).toBeCloseTo(0, 1)
        })
    })

    describe("Edge Cases", () => {
        test("Should handle zero-size bounding rect", () => {
            // Mock getBoundingClientRect to return a zero-size rect
            const zeroRect: DOMRect = {
                left: 100,
                top: 100,
                right: 100,
                bottom: 100,
                width: 0,
                height: 0,
                x: 100,
                y: 100,
                toJSON: () => ({}),
            }
            vi.spyOn(mockBaseElement, "getBoundingClientRect").mockReturnValue(zeroRect)

            const mockPointerEvent = new PointerEvent("pointerdown", {
                clientX: 150,
                clientY: 150,
                pointerId: 1,
            })

            // Should not throw error
            expect(() => {
                mockBaseElement.dispatchEvent(mockPointerEvent)
            }).not.toThrow()
        })

        test("Should handle pointermove without initial pointerdown", () => {
            const pointerMoveEvent = new PointerEvent("pointermove", {
                clientX: 175,
                clientY: 175,
                pointerId: 1,
            })

            // Should not throw error and position should remain at 0
            expect(() => {
                document.dispatchEvent(pointerMoveEvent)
            }).not.toThrow()

            expect(joystick.x).toBe(0)
            expect(joystick.y).toBe(0)
        })

        test("Should handle pointerup without initial pointerdown", () => {
            const pointerUpEvent = new PointerEvent("pointerup", {
                pointerId: 1,
            })

            // Should not throw error
            expect(() => {
                document.dispatchEvent(pointerUpEvent)
            }).not.toThrow()
        })
    })
})
