import { MiraType } from "@/mirabuf/MirabufLoader.ts"

const EVENT_KEY = "ObjectCreatedEvent"

/** Event handler for other SceneOverlay events */
export class MirabufObjectCreatedEvent extends Event {
    readonly objectType: MiraType
    public constructor(type: MiraType) {
        super(EVENT_KEY)
        this.objectType = type
        window.dispatchEvent(this)
    }

    public static Listen(func: (e: MirabufObjectCreatedEvent) => void) {
        window.addEventListener(EVENT_KEY, func as EventListener)
    }

    public static RemoveListener(func: (e: MirabufObjectCreatedEvent) => void) {
        window.removeEventListener(EVENT_KEY, func as EventListener)
    }
}
