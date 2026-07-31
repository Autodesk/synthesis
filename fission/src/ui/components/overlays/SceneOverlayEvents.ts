import EventSystem from "@/systems/EventSystem.ts"
import type { Alliance } from "@/systems/preferences/PreferenceTypes.ts"

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
    public text: () => string
    public color?: Alliance
    public position: PixelSpaceCoord // Screen Space

    public get id() {
        return this._id
    }

    /** Create a new tag */
    public constructor(
        text: () => string,
        public isOwn: () => boolean,
        position?: PixelSpaceCoord,
        color?: Alliance
    ) {
        this._id = nextTagId++

        this.text = text
        this.position = position ?? [0, 0]
        this.color = color
        EventSystem.dispatch("SceneOverlayTagAddEvent", this)
    }

    /** Removing the tag */
    public dispose() {
        EventSystem.dispatch("SceneOverlayTagRemoveEvent", this)
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
