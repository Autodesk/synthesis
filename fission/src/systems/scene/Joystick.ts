class Joystick {
    private baseElement: HTMLElement
    private stickElement: HTMLElement
    private maxDistance: number
    private dragStart: { x: number; y: number } | null = null
    private stickPosition: { x: number; y: number } = { x: 0, y: 0 }
    private movementCallback: (x: number, y: number) => void

    constructor(
        baseElement: HTMLElement,
        stickElement: HTMLElement,
        maxDistance: number,
        movementCallback: (x: number, y: number) => void
    ) {
        this.baseElement = baseElement
        this.stickElement = stickElement
        this.maxDistance = maxDistance
        this.movementCallback = movementCallback

        this.initialize()
    }

    private initialize() {
        this.stickElement.addEventListener("pointerdown", this.onPointerDown.bind(this))
        document.addEventListener("pointermove", this.onPointerMove.bind(this))
        document.addEventListener("pointerup", this.onPointerUp.bind(this))
    }

    private onPointerDown(event: PointerEvent) {
        this.dragStart = { x: event.clientX, y: event.clientY }
    }

    private onPointerMove(event: PointerEvent) {
        if (!this.dragStart) return

        const dx = event.clientX - this.dragStart.x
        const dy = event.clientY - this.dragStart.y

        const distance = Math.sqrt(dx * dx + dy * dy)
        const angle = Math.atan2(dy, dx)

        const limitedDistance = Math.min(this.maxDistance, distance)

        this.stickPosition.x = limitedDistance * Math.cos(angle)
        this.stickPosition.y = limitedDistance * Math.sin(angle)

        this.stickElement.style.transform = `translate(${this.stickPosition.x}px, ${this.stickPosition.y}px)`

        this.movementCallback(this.stickPosition.x / this.maxDistance, this.stickPosition.y / this.maxDistance)
    }

    private onPointerUp() {
        this.dragStart = null
        this.stickPosition = { x: 0, y: 0 }
        this.stickElement.style.transform = `translate(-50%, -50%)`

        this.movementCallback(0, 0)
    }
}

export default Joystick
