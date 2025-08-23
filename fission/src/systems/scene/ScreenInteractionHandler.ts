export const PRIMARY_MOUSE_INTERACTION = 0
export const MIDDLE_MOUSE_INTERACTION = 1
export const SECONDARY_MOUSE_INTERACTION = 2
export const TOUCH_INTERACTION = 3
export const TOUCH_DOUBLE_INTERACTION = 4

export type InteractionType = -1 | 0 | 1 | 2 | 3 | 4

export interface InteractionStart {
    interactionType: InteractionType
    position: [number, number]
}

export interface InteractionMove {
    interactionType: InteractionType
    scale?: number
    movement?: [number, number]
}

export interface InteractionEnd {
    interactionType: InteractionType
    position: [number, number]
}

/**
 * Handler for all screen interactions with Mouse, Pen, and Touch controls.
 */
class ScreenInteractionHandler {
    private _primaryTouch: number | undefined
    private _secondaryTouch: number | undefined
    private _primaryTouchPosition: [number, number] | undefined
    private _secondaryTouchPosition: [number, number] | undefined
    private _movementThresholdMet: boolean = false
    private _doubleTapInteraction: boolean = false
    private _pointerPosition: [number, number] | undefined

    private _lastPinchSeparation: number | undefined
    private _lastPinchPosition: [number, number] | undefined

    private _pointerMove: (ev: PointerEvent) => void
    private _wheelMove: (ev: WheelEvent) => void
    private _pointerDown: (ev: PointerEvent) => void
    private _pointerUp: (ev: PointerEvent) => void
    private _contextMenu: (ev: MouseEvent) => unknown
    private _touchMove: (ev: TouchEvent) => void

    private _domElement: HTMLElement

    private _globalPointerUp: (ev: PointerEvent) => void
    private _isInteracting: boolean = false

    public interactionStart: ((i: InteractionStart) => void) | undefined
    public interactionEnd: ((i: InteractionEnd) => void) | undefined
    public interactionMove: ((i: InteractionMove) => void) | undefined
    public contextMenu: ((i: InteractionEnd) => void) | undefined

    /**
     * Caculates the distance between the primary and secondary touch positions.
     *
     * @returns Distance in pixels. Undefined if primary or secondary touch positions are undefined.
     */
    public get pinchSeparation(): number | undefined {
        if (this._primaryTouchPosition == undefined || this._secondaryTouchPosition == undefined) {
            return undefined
        }

        const diff = [
            this._primaryTouchPosition[0] - this._secondaryTouchPosition[0],
            this._primaryTouchPosition[1] - this._secondaryTouchPosition[1],
        ]
        return Math.sqrt(diff[0] ** 2 + diff[1] ** 2)
    }

    /**
     * Gets the midpoint between the primary and secondary touch positions.
     *
     * @returns Midpoint between primary and secondary touch positions. Undefined if touch positions are undefined.
     */
    public get pinchPosition(): [number, number] | undefined {
        if (this._primaryTouchPosition == undefined || this._secondaryTouchPosition == undefined) {
            return undefined
        }

        return [
            (this._primaryTouchPosition[0] + this._secondaryTouchPosition[0]) / 2.0,
            (this._primaryTouchPosition[1] + this._secondaryTouchPosition[1]) / 2.0,
        ]
    }

    /**
     * Adds event listeners to dom element and wraps interaction events around original dom events.
     *
     * @param domElement Element to attach events to. Generally canvas for our application.
     */
    public constructor(domElement: HTMLElement) {
        this._domElement = domElement

        this._pointerMove = e => this.pointerMove(e)
        this._wheelMove = e => this.wheelMove(e)
        this._pointerDown = e => this.pointerDown(e)
        this._pointerUp = e => this.pointerUp(e)
        this._contextMenu = e => e.preventDefault()
        this._touchMove = e => e.preventDefault()

        this._globalPointerUp = e => {
            if (this._isInteracting) {
                this.pointerUp(e)
            }
        }

        this._domElement.addEventListener("pointermove", this._pointerMove)
        this._domElement.addEventListener(
            "wheel",
            e => {
                if (e.ctrlKey) {
                    e.preventDefault()
                } else {
                    this._wheelMove(e)
                }
            },
            { passive: false }
        )
        this._domElement.addEventListener("contextmenu", this._contextMenu)
        this._domElement.addEventListener("pointerdown", this._pointerDown)
        this._domElement.addEventListener("pointerup", this._pointerUp)
        this._domElement.addEventListener("pointercancel", this._pointerUp)
        this._domElement.addEventListener("pointerleave", this._pointerUp)

        this._domElement.addEventListener("touchmove", this._touchMove, { passive: false })
    }

    /**
     * Disposes attached event handlers on the selected dom element.
     */
    public dispose() {
        this._domElement.removeEventListener("pointermove", this._pointerMove)
        this._domElement.removeEventListener("wheel", this._wheelMove)
        this._domElement.removeEventListener("contextmenu", this._contextMenu)
        this._domElement.removeEventListener("pointerdown", this._pointerDown)
        this._domElement.removeEventListener("pointerup", this._pointerUp)
        this._domElement.removeEventListener("pointercancel", this._pointerUp)
        this._domElement.removeEventListener("pointerleave", this._pointerUp)

        this._domElement.removeEventListener("touchmove", this._touchMove)

        if (this._isInteracting) {
            document.removeEventListener("pointerup", this._globalPointerUp)
            this._isInteracting = false
        }
    }

    /**
     * This method intercepts pointer move events and translates them into interaction move events accordingly. Pen and mouse movements have
     * very minimal parsing, while touch movements are split into two categories. Either you have only a primary touch on the screen, in which
     * it has, again, very minimal parsing. However, if there is a secondary touch, it simply updates the tracked positions, without dispatching
     * any events. The touches positions are then translated into pinch and pan movements inside the update method.
     *
     * Pointer movements need to move half the recorded pointers width or height (depending on direction of movement) in order to begin updating
     * the position data and dispatch events.
     *
     * @param e Pointer Event data.
     */
    private pointerMove(e: PointerEvent) {
        if (!this.interactionMove) {
            return
        }

        if (e.pointerType == "mouse" || e.pointerType == "pen") {
            if (this._pointerPosition && !this._movementThresholdMet) {
                const delta = [
                    Math.abs(e.clientX - this._pointerPosition![0]),
                    Math.abs(e.clientY - this._pointerPosition![1]),
                ]
                if (delta[0] > window.innerWidth * 0.01 || delta[1] > window.innerHeight * 0.01) {
                    this._movementThresholdMet = true
                } else {
                    return
                }
            }

            this._pointerPosition = [e.movementX, e.movementY]

            this.interactionMove({
                interactionType: e.button as InteractionType,
                movement: [e.movementX, e.movementY],
            })
        } else {
            if (e.pointerId == this._primaryTouch) {
                if (!this._movementThresholdMet) {
                    if (this.checkMovementThreshold(this._primaryTouchPosition!, e)) {
                        this._movementThresholdMet = true
                    } else {
                        return
                    }
                }

                this._primaryTouchPosition = [e.clientX, e.clientY]

                if (this._secondaryTouch == undefined) {
                    this.interactionMove({
                        interactionType: PRIMARY_MOUSE_INTERACTION,
                        movement: [e.movementX, e.movementY],
                    })
                }
            } else if (e.pointerId == this._secondaryTouch) {
                if (!this._movementThresholdMet) {
                    if (this.checkMovementThreshold(this._secondaryTouchPosition!, e)) {
                        this._movementThresholdMet = true
                    } else {
                        return
                    }
                }

                this._secondaryTouchPosition = [e.clientX, e.clientY]
            }
        }
    }

    /**
     * Intercepts wheel events and passes them along via the interaction move event.
     *
     * @param e Wheel event data.
     */
    private wheelMove(e: WheelEvent) {
        if (!this.interactionMove) {
            return
        }

        this.interactionMove({ interactionType: -1, scale: e.deltaY * 0.01 })
    }

    /**
     * The primary role of update within screen interaction handler is to parse the double touches on the screen into
     * pinch and pan movement, then dispatch the data via the interaction move events.
     *
     * @param _ Unused deltaT variable.
     */
    public update(_: number) {
        if (this._secondaryTouch != undefined && this._movementThresholdMet) {
            // Calculate current pinch position and separation
            const pinchSep = this.pinchSeparation!
            const pinchPos = this.pinchPosition!

            // If previous ones exist, determine delta and send events
            if (this._lastPinchPosition != undefined && this._lastPinchSeparation != undefined) {
                this.interactionMove?.({
                    interactionType: SECONDARY_MOUSE_INTERACTION,
                    scale: (pinchSep - this._lastPinchSeparation) * -0.03,
                })

                this.interactionMove?.({
                    interactionType: SECONDARY_MOUSE_INTERACTION,
                    movement: [pinchPos[0] - this._lastPinchPosition[0], pinchPos[1] - this._lastPinchPosition[1]],
                })
            }

            // Load current into last
            this._lastPinchSeparation = pinchSep
            this._lastPinchPosition = pinchPos
        }
    }

    private pointerDown(e: PointerEvent) {
        if (!this.interactionStart) {
            return
        }

        if (!this._isInteracting) {
            this._isInteracting = true
            document.addEventListener("pointerup", this._globalPointerUp)
        }

        if (e.pointerType == "touch") {
            if (this._primaryTouch == undefined) {
                this._primaryTouch = e.pointerId
                this._primaryTouchPosition = [e.clientX, e.clientY]
                this._movementThresholdMet = false
                this.interactionStart({
                    interactionType: PRIMARY_MOUSE_INTERACTION,
                    position: this._primaryTouchPosition,
                })
            } else if (this._secondaryTouch == undefined) {
                this._secondaryTouch = e.pointerId
                this._secondaryTouchPosition = [e.clientX, e.clientY]
                this._doubleTapInteraction = true

                this._lastPinchSeparation = undefined
                this._lastPinchPosition = undefined

                this.interactionStart({
                    interactionType: SECONDARY_MOUSE_INTERACTION,
                    position: this._secondaryTouchPosition,
                })
            }
        } else {
            if (e.button >= 0 && e.button <= 2) {
                this._movementThresholdMet = false
                this._pointerPosition = [e.clientX, e.clientY]
                this.interactionStart({
                    interactionType: e.button as InteractionType,
                    position: [e.clientX, e.clientY],
                })
            }
        }
    }

    private pointerUp(e: PointerEvent) {
        if (!this.interactionEnd) {
            return
        }

        if (e.pointerType == "touch") {
            if (e.pointerId == this._primaryTouch) {
                this._primaryTouch = this._secondaryTouch
                this._secondaryTouch = undefined
                if (this._primaryTouch != undefined) {
                    this.interactionEnd({
                        interactionType: SECONDARY_MOUSE_INTERACTION,
                        position: [e.clientX, e.clientY],
                    })
                } else {
                    this.interactionEnd({
                        interactionType: PRIMARY_MOUSE_INTERACTION,
                        position: [e.clientX, e.clientY],
                    })
                    if (this._doubleTapInteraction && !this._movementThresholdMet && this.contextMenu) {
                        this.contextMenu({
                            interactionType: -1,
                            position: this.pinchPosition!,
                        })
                    }
                    this._doubleTapInteraction = false
                }
                // Reset continuous tracking
            } else if (e.pointerId == this._secondaryTouch) {
                this._secondaryTouch = undefined
                this.interactionEnd({
                    interactionType: SECONDARY_MOUSE_INTERACTION,
                    position: [e.clientX, e.clientY],
                })
            }
        } else {
            if (e.button >= 0 && e.button <= 2) {
                const end: InteractionEnd = {
                    interactionType: e.button as InteractionType,
                    position: [e.clientX, e.clientY],
                }
                this.interactionEnd(end)
                if (e.button == SECONDARY_MOUSE_INTERACTION && !this._movementThresholdMet && this.contextMenu) {
                    this.contextMenu(end)
                }

                this._pointerPosition = undefined
            }
        }

        if (
            this._isInteracting &&
            this._primaryTouch === undefined &&
            this._secondaryTouch === undefined &&
            this._pointerPosition === undefined
        ) {
            this._isInteracting = false
            document.removeEventListener("pointerup", this._globalPointerUp)
        }
    }

    /**
     * Checks if a given position has moved from the origin given a specified threshold.
     *
     * @param origin Origin to move away from.
     * @param ptr Pointer data.
     * @returns True if latest is outside of the box around origin with sides the length of thresholds * 2.
     */
    private checkMovementThreshold(origin: [number, number], ptr: PointerEvent): boolean {
        const delta = [Math.abs(ptr.clientX - origin[0]), Math.abs(ptr.clientY - origin[1])]

        return delta[0] > ptr.width / 2.0 || delta[1] > ptr.height / 2.0
    }
}

export default ScreenInteractionHandler
