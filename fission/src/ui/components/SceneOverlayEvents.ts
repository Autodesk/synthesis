let nextTagId = 0

/* Coordinates for tags in world space */
export type PixelSpaceCoord = [number, number]

/**
 * Represents a tag that can be displayed on the screen
 *
 * @param text The text to display
 * @param position The position of the tag in screen space (default: [0,0])
 */
export class SceneOverlayTag {
    private _id: number
    public text: string
    public position: PixelSpaceCoord // Screen Space

    public get id() {
        return this._id
    }

    /** Create a new tag */
    public constructor(text: string, position?: PixelSpaceCoord) {
        this._id = nextTagId++

        this.text = text
        this.position = position ?? [0, 0]
        new SceneOverlayTagAddEvent(this)
    }

    /** Removing the tag */
    public Dispose() {
        new SceneOverlayTagRemoveEvent(this)
    }
}

/**
 * Event handler that is run when a SceneOverlayTag is updated
 */
export class SceneOverlayTagAddEvent extends Event {
    private static readonly EVENT_KEY = "SceneOverlayTagEvent"

    public tag: SceneOverlayTag

    /** Create a new event bound to a tag */
    public constructor(tag: SceneOverlayTag) {
        super(SceneOverlayTagAddEvent.EVENT_KEY)

        this.tag = tag

        window.dispatchEvent(this)
    }

    /** Listener for tag updates */
    public static Listen(func: (e: Event) => void) {
        window.addEventListener(SceneOverlayTagAddEvent.EVENT_KEY, func)
    }

    /** Removing listener */
    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(SceneOverlayTagAddEvent.EVENT_KEY, func)
    }
}

export class SceneOverlayTagRemoveEvent extends Event {
    private static readonly EVENT_KEY = "SceneOverlayTagRemoveEvent"

    public tag: SceneOverlayTag

    /** Create a new event bound to a tag */
    public constructor(tag: SceneOverlayTag) {
        super(SceneOverlayTagRemoveEvent.EVENT_KEY)

        this.tag = tag

        window.dispatchEvent(this)
    }

    /** Listener for tag updates */
    public static Listen(func: (e: Event) => void) {
        window.addEventListener(SceneOverlayTagRemoveEvent.EVENT_KEY, func)
    }

    /** Removing listener */
    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(SceneOverlayTagRemoveEvent.EVENT_KEY, func)
    }
}
