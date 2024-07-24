import * as THREE from "three"

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
    scale?: number,
    movement?: [number, number]
}

export interface InteractionEnd {
    interactionType: InteractionType
    position: [number, number]
}

/**
 * Handler for all screen interactions with Mouse, Pen, and Touch controls.
 * 
 * Mouse and Pen controls are stateless, whereas Touch controls are stateful
 */
class ScreenInteractionHandler {

    private _primaryTouch: number | undefined
    private _secondaryTouch: number | undefined
    private _primaryTouchPosition: [number, number] | undefined
    private _secondaryTouchPosition: [number, number] | undefined
    private _movementThresholdMet: boolean = false
    private _doubleTapInteraction: boolean = false

    private _pointerMove: (ev: PointerEvent) => void
    private _wheelMove: (ev: WheelEvent) => void
    private _pointerDown: (ev: PointerEvent) => void
    private _pointerUp: (ev: PointerEvent) => void
    private _contextMenu: (ev: MouseEvent) => unknown
    private _touchMove: (ev: TouchEvent) => void

    private _domElement: HTMLElement

    public interactionStart: ((i: InteractionStart) => void) | undefined
    public interactionEnd: ((i: InteractionEnd) => void) | undefined
    public interactionMove: ((i: InteractionMove) => void) | undefined
    public contextMenu: ((i: InteractionEnd) => void) | undefined

    public constructor(domElement: HTMLElement) {
        this._domElement = domElement

        this._pointerMove = e => this.pointerMove(e) 
        this._wheelMove = e => this.wheelMove(e)
        this._pointerDown = e => this.pointerDown(e)
        this._pointerUp = e => this.pointerUp(e)
        this._contextMenu = e => e.preventDefault()
        this._touchMove = e => e.preventDefault()
        
        this._domElement.addEventListener("pointermove", this._pointerMove)
        this._domElement.addEventListener("wheel", this._wheelMove, { passive: false })
        this._domElement.addEventListener("contextmenu", this._contextMenu)
        this._domElement.addEventListener("pointerdown", this._pointerDown)
        this._domElement.addEventListener("pointerup", this._pointerUp)
        this._domElement.addEventListener("pointercancel", this._pointerUp)
        this._domElement.addEventListener("pointerleave", this._pointerUp)

        this._domElement.addEventListener("touchmove", this._touchMove)
    }

    public dispose() {
        this._domElement.removeEventListener("pointermove", this._pointerMove)
        this._domElement.removeEventListener("wheel", this._wheelMove)
        this._domElement.removeEventListener("contextmenu", this._contextMenu)
        this._domElement.removeEventListener("pointerdown", this._pointerDown)
        this._domElement.removeEventListener("pointerup", this._pointerUp)
        this._domElement.removeEventListener("pointercancel", this._pointerUp)
        this._domElement.removeEventListener("pointerleave", this._pointerUp)

        this._domElement.removeEventListener("touchmove", this._touchMove)
    }

    private pointerMove(e: PointerEvent) {
        if (!this.interactionMove) {
            return
        }
        
        if (e.pointerType == "mouse" || e.pointerType == "pen") {
            this.interactionMove({ interactionType: e.button as InteractionType, movement: [e.movementX, e.movementY] })
        } else {
            if (e.pointerId == this._primaryTouch) {

                if (!this._movementThresholdMet) {
                    const delta = [Math.abs(e.clientX - this._primaryTouchPosition![0]), Math.abs(e.clientY - this._primaryTouchPosition![1])]
                    if (delta[0] > e.width || delta[1] > e.height) {
                        this._movementThresholdMet = true
                    } else {
                        return
                    }
                }

                this._primaryTouchPosition = [e.clientX, e.clientY]
                this.interactionMove({
                    interactionType: PRIMARY_MOUSE_INTERACTION,
                    movement: [e.movementX, e.movementY]
                })
            } else if (e.pointerId == this._secondaryTouch) {
                
                if (!this._movementThresholdMet) {
                    const delta = [Math.abs(e.clientX - this._secondaryTouchPosition![0]), Math.abs(e.clientY - this._secondaryTouchPosition![1])]
                    if (delta[0] > e.width || delta[1] > e.height) {
                        this._movementThresholdMet = true
                    } else {
                        return
                    }
                }

                this._secondaryTouchPosition = [e.clientX, e.clientY]
                if (this._primaryTouchPosition) { // This shouldn't happen, but you never know
                    const scalingDir = (new THREE.Vector2(this._secondaryTouchPosition[0], this._secondaryTouchPosition[1])).sub(
                        new THREE.Vector2(this._primaryTouchPosition[0], this._primaryTouchPosition[1])
                    )

                    const scale = scalingDir.normalize().dot(new THREE.Vector2(e.movementX, e.movementY))

                    this.interactionMove({
                        interactionType: PRIMARY_MOUSE_INTERACTION,
                        scale: scale * -0.06
                    })
                }
            }
        }
    }

    private wheelMove(e: WheelEvent) {
        if (!this.interactionMove) {
            return
        }

        this.interactionMove({ interactionType: -1, scale: e.deltaY * 0.01 })
    }

    private pointerDown(e: PointerEvent) {
        if (!this.interactionStart) {
            return
        }

        if (e.pointerType == "touch") {
            if (!this._primaryTouch) {
                this._primaryTouch = e.pointerId
                this._primaryTouchPosition = [e.clientX, e.clientY]
                this.interactionStart({ interactionType: PRIMARY_MOUSE_INTERACTION, position: this._primaryTouchPosition })
            } else if (!this._secondaryTouch) {
                this._secondaryTouch = e.pointerId
                this._secondaryTouchPosition = [e.clientX, e.clientY]
                this._doubleTapInteraction = true
            }
        } else {
            if (e.button >= 0 && e.button <= 2) {
                this.interactionStart({ interactionType: e.button as InteractionType, position: [e.clientX, e.clientY] })
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
                if (!this._primaryTouch) {
                    const end: InteractionEnd = { interactionType: PRIMARY_MOUSE_INTERACTION, position: [e.clientX, e.clientY] }
                    this.interactionEnd(end)
                    if (this._doubleTapInteraction && this.contextMenu) {
                        this.contextMenu(end)
                    }
                }
                // Reset continuous tracking
            } else if (e.pointerId == this._secondaryTouch) {
                this._secondaryTouch = undefined
                // Reset continuous tracking
            }
        } else {
            if (e.button >= 0 && e.button <= 2) {
                this.interactionEnd({ interactionType: e.button as InteractionType, position: [e.clientX, e.clientY] })
            }
        }
    }
}

export default ScreenInteractionHandler