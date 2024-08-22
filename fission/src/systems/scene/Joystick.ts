import { MAX_JOYSTICK_RADIUS } from "@/ui/components/TouchControls"

class Joystick {
    private baseElement: HTMLElement
    private stickElement: HTMLElement
    private stickPosition: { x: number; y: number } = { x: 0, y: 0 }
    private baseRect: DOMRect | null = null
    private activePointerId: number | null = null // Track the active pointer ID

    public get x() {
        return this.stickPosition.x / MAX_JOYSTICK_RADIUS
    }

    public get y() {
        return this.stickPosition.y / MAX_JOYSTICK_RADIUS
    }

    constructor(baseElement: HTMLElement, stickElement: HTMLElement) {
        this.baseElement = baseElement
        this.stickElement = stickElement

        this.initialize()
    }

    private initialize() {
        this.baseElement.addEventListener("pointerdown", this.onPointerDown.bind(this))
        document.addEventListener("pointermove", this.onPointerMove.bind(this))
        document.addEventListener("pointerup", this.onPointerUp.bind(this))
    }

    private onPointerDown(event: PointerEvent) {
        this.baseRect = this.baseElement.getBoundingClientRect()
        this.activePointerId = event.pointerId
        this.updateStickPosition(event.clientX, event.clientY)
    }

    private onPointerMove(event: PointerEvent) {
        if (this.activePointerId !== event.pointerId || !this.baseRect) return // Ensure only the initiating pointer controls the joystick
        this.updateStickPosition(event.clientX, event.clientY)
    }

    private onPointerUp(event: PointerEvent) {
        if (this.activePointerId !== event.pointerId) return
        this.stickPosition = { x: 0, y: 0 }
        this.stickElement.style.transform = `translate(-50%, -50%)`
        this.baseRect = null
    }

    private updateStickPosition(clientX: number, clientY: number) {
        if (!this.baseRect) return

        const w = this.baseRect.right - this.baseRect.left
        const h = this.baseRect.bottom - this.baseRect.top
        const x = clientX - (this.baseRect.left + w / 2)
        const y = clientY - (this.baseRect.top + h / 2)

        // Calculate the distance from the center
        const distance = Math.sqrt(x * x + y * y)

        // If the distance exceeds maxDistance, constrain it
        if (distance > MAX_JOYSTICK_RADIUS) {
            const angle = Math.atan2(y, x)
            this.stickPosition.x = Math.cos(angle) * MAX_JOYSTICK_RADIUS
            this.stickPosition.y = Math.sin(angle) * MAX_JOYSTICK_RADIUS
        } else {
            this.stickPosition.x = x
            this.stickPosition.y = y
        }

        this.stickElement.style.transform = `translate(${this.stickPosition.x - MAX_JOYSTICK_RADIUS / 2}px, ${this.stickPosition.y - MAX_JOYSTICK_RADIUS / 2}px)`
    }

    public GetPosition(axis: "x" | "y") {
        return this.stickPosition[axis] / MAX_JOYSTICK_RADIUS
    }
}

export default Joystick
