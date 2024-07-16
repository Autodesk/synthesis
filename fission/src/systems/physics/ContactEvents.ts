import Jolt from "@barclah/jolt-physics";

export interface CurrentContactData {
    body1: Jolt.Body
    body2: Jolt.Body
    manifold: Jolt.ContactManifold
    settings: Jolt.ContactSettings
}

export interface OnContactValidateData {
    body1: Jolt.Body
    body2: Jolt.Body
    baseOffset: Jolt.RVec3
    collisionResult: Jolt.CollideShapeResult
}

export class OnContactAddedEvent extends Event {
    public static readonly EVENT_KEY = 'OnContactAddedEvent'

    public message: CurrentContactData

    public constructor(data: CurrentContactData) {
        super(OnContactAddedEvent.EVENT_KEY)

        this.message = data
    }

    public static Dispatch(data: CurrentContactData) {
        window.dispatchEvent(new OnContactAddedEvent(data))
    }

    public static AddListener(func: (e: Event) => void) {
        window.addEventListener(OnContactAddedEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(OnContactAddedEvent.EVENT_KEY, func)
    }
}

export class OnContactPersistedEvent extends Event {
    public static readonly EVENT_KEY = 'OnContactPersistedEvent'

    public message: CurrentContactData

    public constructor(data: CurrentContactData) {
        super(OnContactPersistedEvent.EVENT_KEY)

        this.message = data
    }

    public static Dispatch(data: CurrentContactData) {
        window.dispatchEvent(new OnContactPersistedEvent(data))
    }

    public static AddListener(func: (e: Event) => void) {
        window.addEventListener(OnContactPersistedEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(OnContactPersistedEvent.EVENT_KEY, func)
    }
}

export class OnContactRemovedEvent extends Event {
    public static readonly EVENT_KEY = 'OnContactRemovedEvent'

    public message: Jolt.SubShapeIDPair

    public constructor(data: Jolt.SubShapeIDPair) {
        super(OnContactRemovedEvent.EVENT_KEY)

        this.message = data
    }

    public static Dispatch(data: Jolt.SubShapeIDPair) {
        window.dispatchEvent(new OnContactRemovedEvent(data))
    }

    public static AddListener(func: (e: Event) => void) {
        window.addEventListener(OnContactRemovedEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(OnContactRemovedEvent.EVENT_KEY, func)
    }
}

export class OnContactValidateEvent extends Event {
    public static readonly EVENT_KEY = 'OnContactValidateEvent'

    public message: OnContactValidateData

    public constructor(data: OnContactValidateData) {
        super(OnContactValidateEvent.EVENT_KEY)

        this.message = data
    }

    public static Dispatch(data: OnContactValidateData) {
        window.dispatchEvent(new OnContactValidateEvent(data))
    }

    public static AddListener(func: (e: Event) => void) {
        window.addEventListener(OnContactValidateEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(OnContactValidateEvent.EVENT_KEY, func)
    }
}