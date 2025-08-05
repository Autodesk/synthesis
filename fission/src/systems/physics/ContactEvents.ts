import type Jolt from "@azaleacolburn/jolt-physics"

export interface CurrentContactData {
    body1: Jolt.BodyID
    body2: Jolt.BodyID
    manifold: Jolt.ContactManifold
    settings: Jolt.ContactSettings
}

export interface OnContactValidateData {
    body1: Jolt.Body
    body2: Jolt.Body
    baseOffset: Jolt.RVec3
    collisionResult: Jolt.CollideShapeResult
}

export abstract class PhysicsEvent extends Event {
    abstract dispatch(): void
}

export class OnContactAddedEvent extends PhysicsEvent {
    public static readonly EVENT_KEY = "OnContactAddedEvent"

    public message: CurrentContactData

    public constructor(data: CurrentContactData) {
        super(OnContactAddedEvent.EVENT_KEY)

        this.message = data
    }

    public dispatch(): void {
        window.dispatchEvent(this)
    }

    public static addListener(func: (e: OnContactAddedEvent) => void) {
        window.addEventListener(OnContactAddedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: OnContactAddedEvent) => void) {
        window.removeEventListener(OnContactAddedEvent.EVENT_KEY, func as (e: Event) => void)
    }
}

export class OnContactPersistedEvent extends PhysicsEvent {
    public static readonly EVENT_KEY = "OnContactPersistedEvent"

    public message: CurrentContactData

    public constructor(data: CurrentContactData) {
        super(OnContactPersistedEvent.EVENT_KEY)

        this.message = data
    }

    public dispatch(): void {
        window.dispatchEvent(this)
    }

    public static addListener(func: (e: OnContactPersistedEvent) => void) {
        window.addEventListener(OnContactPersistedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: OnContactPersistedEvent) => void) {
        window.removeEventListener(OnContactPersistedEvent.EVENT_KEY, func as (e: Event) => void)
    }
}

// This one is special because having it in the queue in PhysicsSystem causes issues with scoring
export class OnContactRemovedEvent extends Event {
    public static readonly EVENT_KEY = "OnContactRemovedEvent"

    public message: Jolt.SubShapeIDPair

    public constructor(data: Jolt.SubShapeIDPair) {
        super(OnContactRemovedEvent.EVENT_KEY)

        this.message = data

        window.dispatchEvent(this)
    }

    public static addListener(func: (e: OnContactRemovedEvent) => void) {
        window.addEventListener(OnContactRemovedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: OnContactRemovedEvent) => void) {
        window.removeEventListener(OnContactRemovedEvent.EVENT_KEY, func as (e: Event) => void)
    }
}

export class OnContactValidateEvent extends PhysicsEvent {
    public static readonly EVENT_KEY = "OnContactValidateEvent"

    public message: OnContactValidateData

    public constructor(data: OnContactValidateData) {
        super(OnContactValidateEvent.EVENT_KEY)

        this.message = data
    }

    public dispatch(): void {
        window.dispatchEvent(this)
    }

    public static addListener(func: (e: OnContactValidateEvent) => void) {
        window.addEventListener(OnContactValidateEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static removeListener(func: (e: OnContactValidateEvent) => void) {
        window.removeEventListener(OnContactValidateEvent.EVENT_KEY, func as (e: Event) => void)
    }
}
