import type { Alliance } from "@/systems/preferences/PreferenceTypes.ts"

let nextTagId = 0

/* Coordinates for tags in world space */
export type PixelSpaceCoord = [number, number]

/** Contains the event keys for events that require a SceneOverlayTag as a parameter */
export const enum SceneOverlayTagEventKey {
    ADD = "SceneOverlayTagAddEvent",
    REMOVE = "SceneOverlayTagRemoveEvent",
}

/** Contains the event keys for other Scene Overlay Events */
export const enum SceneOverlayEventKey {
    UPDATE = "SceneOverlayUpdateEvent",
    DISABLE = "SceneOverlayDisableEvent",
    ENABLE = "SceneOverlayEnableEvent",
}

/**
 * Represents a tag that can be displayed on the screen
 *
 * @param text The text to display
 * @param position The position of the tag in screen space (default: [0,0])
 */

export class SceneOverlayTag {
    private _id: number
    public text: () => string
    public color?: Alliance
    public position: PixelSpaceCoord // Screen Space

    public get id() {
        return this._id
    }

    /** Create a new tag */
    public constructor(text: () => string, position?: PixelSpaceCoord, color?: Alliance) {
        this._id = nextTagId++

        this.text = text
        this.position = position ?? [0, 0]
        this.color = color
        new SceneOverlayTagEvent(SceneOverlayTagEventKey.ADD, this)
    }

    /** Removing the tag */
    public dispose() {
        new SceneOverlayTagEvent(SceneOverlayTagEventKey.REMOVE, this)
    }

    public getCSSColor(): string {
        switch (this.color) {
            case "red":
                return "rgba(166,22,27,0.5)"
            case "blue":
                return "rgba(0,74,129,0.5)"
            default:
                return "rgba(0,0,0,0.5)"
        }
    }
}

/** Event handler for events that use a SceneOverlayTag as a parameter */
export class SceneOverlayTagEvent extends Event {
    public tag: SceneOverlayTag

    public constructor(eventKey: SceneOverlayTagEventKey, tag: SceneOverlayTag) {
        super(eventKey)

        this.tag = tag

        window.dispatchEvent(this)
    }

    public static listen(eventKey: SceneOverlayTagEventKey, func: (e: Event) => void) {
        window.addEventListener(eventKey, func)
    }

    public static removeListener(eventKey: SceneOverlayTagEventKey, func: (e: Event) => void) {
        window.removeEventListener(eventKey, func)
    }
}

/** Event handler for other SceneOverlay events */
export class SceneOverlayEvent extends Event {
    public constructor(eventKey: SceneOverlayEventKey) {
        super(eventKey)

        window.dispatchEvent(this)
    }

    public static listen(eventKey: SceneOverlayEventKey, func: (e: Event) => void) {
        window.addEventListener(eventKey, func)
    }

    public static removeListener(eventKey: SceneOverlayEventKey, func: (e: Event) => void) {
        window.removeEventListener(eventKey, func)
    }
}
