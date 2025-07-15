import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import ScreenInteractionHandler, {
    PRIMARY_MOUSE_INTERACTION,
    SECONDARY_MOUSE_INTERACTION,
} from "../../systems/scene/ScreenInteractionHandler"

describe("ScreenInteractionHandler", () => {
    let handler: ScreenInteractionHandler
    let mockElement: HTMLElement
    let mockCallbacks: {
        interactionStart: ReturnType<typeof vi.fn>
        interactionMove: ReturnType<typeof vi.fn>
        interactionEnd: ReturnType<typeof vi.fn>
        contextMenu: ReturnType<typeof vi.fn>
    }

    // Helper function to get event handler from mock calls
    const getEventHandler = (eventType: string): ((event: unknown) => void) => {
        const mockCalls = (mockElement.addEventListener as unknown as { mock: { calls: unknown[][] } }).mock.calls
        const call = mockCalls.find((call: unknown[]) => call[0] === eventType)
        return call?.[1] as (event: unknown) => void
    }

    beforeEach(() => {
        // Create mock DOM element
        mockElement = {
            addEventListener: vi.fn(),
            removeEventListener: vi.fn(),
        } as unknown as HTMLElement

        // Mock window dimensions for threshold calculations
        Object.defineProperty(window, "innerWidth", { value: 1000, writable: true })
        Object.defineProperty(window, "innerHeight", { value: 600, writable: true })

        // Create handler
        handler = new ScreenInteractionHandler(mockElement)

        // Setup mock callbacks
        mockCallbacks = {
            interactionStart: vi.fn(),
            interactionMove: vi.fn(),
            interactionEnd: vi.fn(),
            contextMenu: vi.fn(),
        }

        handler.interactionStart = mockCallbacks.interactionStart
        handler.interactionMove = mockCallbacks.interactionMove
        handler.interactionEnd = mockCallbacks.interactionEnd
        handler.contextMenu = mockCallbacks.contextMenu
    })

    afterEach(() => {
        handler.dispose()
        vi.clearAllMocks()
    })

    test("constructor attaches event listeners", () => {
        expect(mockElement.addEventListener).toHaveBeenCalledWith("pointermove", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("wheel", expect.any(Function), { passive: false })
        expect(mockElement.addEventListener).toHaveBeenCalledWith("contextmenu", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("pointerdown", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("pointerup", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("pointercancel", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("pointerleave", expect.any(Function))
        expect(mockElement.addEventListener).toHaveBeenCalledWith("touchmove", expect.any(Function))
    })

    test("dispose removes event listeners", () => {
        handler.dispose()

        expect(mockElement.removeEventListener).toHaveBeenCalledWith("pointermove", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("wheel", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("contextmenu", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("pointerdown", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("pointerup", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("pointercancel", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("pointerleave", expect.any(Function))
        expect(mockElement.removeEventListener).toHaveBeenCalledWith("touchmove", expect.any(Function))
    })

    describe("Mouse interactions", () => {
        test("handles mouse down and up events", () => {
            const mockPointerDown = {
                pointerType: "mouse",
                button: PRIMARY_MOUSE_INTERACTION,
                clientX: 100,
                clientY: 200,
                pointerId: 1,
            } as PointerEvent

            const mockPointerUp = {
                pointerType: "mouse",
                button: PRIMARY_MOUSE_INTERACTION,
                clientX: 100,
                clientY: 200,
                pointerId: 1,
            } as PointerEvent

            // Simulate pointer down
            const pointerDownHandler = getEventHandler("pointerdown")
            pointerDownHandler(mockPointerDown)

            expect(mockCallbacks.interactionStart).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                position: [100, 200],
            })

            // Simulate pointer up
            const pointerUpHandler = getEventHandler("pointerup")
            pointerUpHandler(mockPointerUp)

            expect(mockCallbacks.interactionEnd).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                position: [100, 200],
            })
        })

        test("handles mouse movement", () => {
            const mockPointerDown = {
                pointerType: "mouse",
                button: PRIMARY_MOUSE_INTERACTION,
                clientX: 100,
                clientY: 200,
                pointerId: 1,
            } as PointerEvent

            const mockPointerMove = {
                pointerType: "mouse",
                button: PRIMARY_MOUSE_INTERACTION,
                clientX: 120,
                clientY: 220,
                movementX: 20,
                movementY: 20,
                pointerId: 1,
            } as PointerEvent

            // Start interaction
            const pointerDownHandler = getEventHandler("pointerdown")
            pointerDownHandler(mockPointerDown)

            const pointerMoveHandler = getEventHandler("pointermove")
            pointerMoveHandler(mockPointerMove)

            expect(mockCallbacks.interactionMove).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                movement: [20, 20],
            })
        })

        test("handles right-click context menu", () => {
            const mockPointerDown = {
                pointerType: "mouse",
                button: SECONDARY_MOUSE_INTERACTION,
                clientX: 100,
                clientY: 200,
                pointerId: 1,
            } as PointerEvent

            const mockPointerUp = {
                pointerType: "mouse",
                button: SECONDARY_MOUSE_INTERACTION,
                clientX: 100,
                clientY: 200,
                pointerId: 1,
            } as PointerEvent

            // Start and end interaction without movement
            const pointerDownHandler = getEventHandler("pointerdown")
            const pointerUpHandler = getEventHandler("pointerup")

            pointerDownHandler(mockPointerDown)
            pointerUpHandler(mockPointerUp)

            expect(mockCallbacks.contextMenu).toHaveBeenCalledWith({
                interactionType: SECONDARY_MOUSE_INTERACTION,
                position: [100, 200],
            })
        })
    })

    describe("Touch interactions", () => {
        test("handles single touch interaction", () => {
            const mockTouchDown = {
                pointerType: "touch",
                pointerId: 1,
                clientX: 100,
                clientY: 200,
                width: 20,
                height: 20,
            } as PointerEvent

            const mockTouchUp = {
                pointerType: "touch",
                pointerId: 1,
                clientX: 100,
                clientY: 200,
            } as PointerEvent

            const pointerDownHandler = getEventHandler("pointerdown")
            const pointerUpHandler = getEventHandler("pointerup")

            pointerDownHandler(mockTouchDown)
            expect(mockCallbacks.interactionStart).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                position: [100, 200],
            })

            pointerUpHandler(mockTouchUp)
            expect(mockCallbacks.interactionEnd).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                position: [100, 200],
            })
        })

        test("handles double touch for pinch gestures", () => {
            const mockFirstTouch = {
                pointerType: "touch",
                pointerId: 1,
                clientX: 100,
                clientY: 200,
                width: 20,
                height: 20,
            } as PointerEvent

            const mockSecondTouch = {
                pointerType: "touch",
                pointerId: 2,
                clientX: 200,
                clientY: 200,
                width: 20,
                height: 20,
            } as PointerEvent

            const pointerDownHandler = getEventHandler("pointerdown")

            // First touch
            pointerDownHandler(mockFirstTouch)
            expect(mockCallbacks.interactionStart).toHaveBeenCalledWith({
                interactionType: PRIMARY_MOUSE_INTERACTION,
                position: [100, 200],
            })

            // Second touch
            pointerDownHandler(mockSecondTouch)
            expect(mockCallbacks.interactionStart).toHaveBeenCalledWith({
                interactionType: SECONDARY_MOUSE_INTERACTION,
                position: [200, 200],
            })
        })

        test("calculates pinch separation and position correctly", () => {
            const mockFirstTouch = {
                pointerType: "touch",
                pointerId: 1,
                clientX: 0,
                clientY: 0,
                width: 20,
                height: 20,
            } as PointerEvent

            const mockSecondTouch = {
                pointerType: "touch",
                pointerId: 2,
                clientX: 300,
                clientY: 400,
                width: 20,
                height: 20,
            } as PointerEvent

            const pointerDownHandler = getEventHandler("pointerdown")

            pointerDownHandler(mockFirstTouch)
            pointerDownHandler(mockSecondTouch)

            // Distance between (0,0) and (300,400) should be 500
            expect(handler.pinchSeparation).toBe(500)

            // Midpoint between (0,0) and (300,400) should be (150,200)
            expect(handler.pinchPosition).toEqual([150, 200])
        })
    })

    describe("Wheel interactions", () => {
        test("handles wheel events", () => {
            const mockWheelEvent = {
                deltaY: 100,
                ctrlKey: false,
            } as WheelEvent

            const wheelHandler = getEventHandler("wheel")
            wheelHandler(mockWheelEvent)

            expect(mockCallbacks.interactionMove).toHaveBeenCalledWith({
                interactionType: -1,
                scale: 1.0, // 100 * 0.01
            })
        })

        test("prevents default on ctrl+wheel", () => {
            const mockWheelEvent = {
                deltaY: 100,
                ctrlKey: true,
                preventDefault: vi.fn(),
            } as unknown as WheelEvent

            const wheelHandler = getEventHandler("wheel")
            wheelHandler(mockWheelEvent)

            expect(mockWheelEvent.preventDefault).toHaveBeenCalled()
            expect(mockCallbacks.interactionMove).not.toHaveBeenCalled()
        })
    })

    describe("Update method for pinch gestures", () => {
        test("dispatches pinch events during update", () => {
            // Set up two touches
            const mockFirstTouch = {
                pointerType: "touch",
                pointerId: 1,
                clientX: 100,
                clientY: 100,
                width: 20,
                height: 20,
                movementX: 50,
                movementY: 50,
            } as PointerEvent

            const mockSecondTouch = {
                pointerType: "touch",
                pointerId: 2,
                clientX: 200,
                clientY: 200,
                width: 20,
                height: 20,
                movementX: 50,
                movementY: 50,
            } as PointerEvent

            const pointerDownHandler = getEventHandler("pointerdown")
            const pointerMoveHandler = getEventHandler("pointermove")

            // Start two touches
            pointerDownHandler(mockFirstTouch)
            pointerDownHandler(mockSecondTouch)

            // Create large movement to exceed threshold
            const mockFirstTouchMoved = {
                ...mockFirstTouch,
                clientX: 150,
                clientY: 150,
                movementX: 50,
                movementY: 50,
            } as PointerEvent

            const mockSecondTouchMoved = {
                ...mockSecondTouch,
                clientX: 250,
                clientY: 250,
                movementX: 50,
                movementY: 50,
            } as PointerEvent

            // Move both touches to trigger movement threshold
            pointerMoveHandler(mockFirstTouchMoved)
            pointerMoveHandler(mockSecondTouchMoved)

            // First update call establishes baseline pinch state
            handler.update(0.016)

            // Clear previous calls to focus on pinch events
            mockCallbacks.interactionMove.mockClear()

            // Move touches closer together to create pinch gesture
            const mockFirstTouchPinched = {
                ...mockFirstTouchMoved,
                clientX: 160,
                clientY: 160,
                movementX: 10,
                movementY: 10,
            } as PointerEvent

            const mockSecondTouchPinched = {
                ...mockSecondTouchMoved,
                clientX: 240,
                clientY: 240,
                movementX: -10,
                movementY: -10,
            } as PointerEvent

            pointerMoveHandler(mockFirstTouchPinched)
            pointerMoveHandler(mockSecondTouchPinched)

            // Second update call should generate pinch events
            handler.update(16)

            // Should have generated pinch scale and movement events
            expect(mockCallbacks.interactionMove).toHaveBeenCalledWith(
                expect.objectContaining({
                    interactionType: SECONDARY_MOUSE_INTERACTION,
                    scale: expect.any(Number),
                })
            )
        })
    })

    test("returns undefined for pinch properties with single touch", () => {
        const mockTouchDown = {
            pointerType: "touch",
            pointerId: 1,
            clientX: 100,
            clientY: 100,
            width: 20,
            height: 20,
        } as PointerEvent

        const pointerDownHandler = getEventHandler("pointerdown")
        pointerDownHandler(mockTouchDown)

        expect(handler.pinchSeparation).toBeUndefined()
        expect(handler.pinchPosition).toBeUndefined()
    })
})
