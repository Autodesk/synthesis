import { MAX_JOYSTICK_RADIUS } from "@/ui/components/TouchControls"

class Joystick {
    private _baseElement: HTMLElement
    private _stickElement: HTMLElement
    private _stickPosition: { x: number; y: number } = { x: 0, y: 0 }
    private _baseRect: DOMRect | null = null
    private _activePointerId: number | null = null // Track the active pointer ID

    public get x() {
        return this._stickPosition.x / MAX_JOYSTICK_RADIUS
    }

    public get y() {
        return this._stickPosition.y / MAX_JOYSTICK_RADIUS
    }

    constructor(baseElement: HTMLElement, stickElement: HTMLElement) {
        this._baseElement = baseElement
        this._stickElement = stickElement

        this.initialize()
    }

    private initialize() {
        this._baseElement.addEventListener("pointerdown", this.onPointerDown.bind(this))
        document.addEventListener("pointermove", this.onPointerMove.bind(this))
        document.addEventListener("pointerup", this.onPointerUp.bind(this))
    }

    private onPointerDown(event: PointerEvent) {
        this._baseRect = this._baseElement.getBoundingClientRect()
        this._activePointerId = event.pointerId
        this.updateStickPosition(event.clientX, event.clientY)
    }

    private onPointerMove(event: PointerEvent) {
        if (this._activePointerId !== event.pointerId || !this._baseRect) return // Ensure only the initiating pointer controls the joystick
        this.updateStickPosition(event.clientX, event.clientY)
    }

    private onPointerUp(event: PointerEvent) {
        if (this._activePointerId !== event.pointerId) return
        this._stickPosition = { x: 0, y: 0 }
        this._stickElement.style.transform = `translate(-50%, -50%)`
        this._baseRect = null
    }

    private updateStickPosition(clientX: number, clientY: number) {
        if (!this._baseRect) return

        const w = this._baseRect.right - this._baseRect.left
        const h = this._baseRect.bottom - this._baseRect.top
        const x = clientX - (this._baseRect.left + w / 2)
        const y = clientY - (this._baseRect.top + h / 2)

        // Calculate the distance from the center
        const distance = Math.sqrt(x * x + y * y)

        // If the distance exceeds maxDistance, constrain it
        if (distance > MAX_JOYSTICK_RADIUS) {
            const angle = Math.atan2(y, x)
            this._stickPosition.x = Math.cos(angle) * MAX_JOYSTICK_RADIUS
            this._stickPosition.y = Math.sin(angle) * MAX_JOYSTICK_RADIUS
        } else {
            this._stickPosition.x = x
            this._stickPosition.y = y
        }

        this._stickElement.style.transform = `translate(${this._stickPosition.x - MAX_JOYSTICK_RADIUS / 2}px, ${this._stickPosition.y - MAX_JOYSTICK_RADIUS / 2}px)`
    }

    public getPosition(axis: "x" | "y") {
        return this._stickPosition[axis] / MAX_JOYSTICK_RADIUS
    }
}

export default Joystick
