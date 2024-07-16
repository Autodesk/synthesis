let nextTagId = 0;

export type PixelSpaceCoord = [number, number]

export class SceneOverlayTag {
    private _id: number
    public text: string
    public position: PixelSpaceCoord // Screen Space

    public get id() { return this._id }

    public constructor(text: string, position?: PixelSpaceCoord) {
        this._id = nextTagId++

        this.text = text
        this.position = position ?? [0,0]
    }

    public Update() {
        new SceneOverlayTagEvent(this)
    }
}

export class SceneOverlayTagEvent extends Event {
    private static readonly EVENT_KEY = "SceneOverlayTagEvent"

    public tag: SceneOverlayTag

    public constructor(tag: SceneOverlayTag) {
        super(SceneOverlayTagEvent.EVENT_KEY)

        this.tag = tag

        window.dispatchEvent(this)
    }

    public static Listen(func: (e: Event) => void) {
        window.addEventListener(SceneOverlayTagEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(SceneOverlayTagEvent.EVENT_KEY, func)
    }
}