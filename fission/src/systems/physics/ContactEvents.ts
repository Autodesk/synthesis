import Jolt from "@barclah/jolt-physics"

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
    abstract Dispatch(): void
}

export class OnContactAddedEvent extends PhysicsEvent {
    public static readonly EVENT_KEY = "OnContactAddedEvent"

    public message: CurrentContactData

    public constructor(data: CurrentContactData) {
        super(OnContactAddedEvent.EVENT_KEY)

        this.message = data
    }

    public Dispatch(): void {
        window.dispatchEvent(this)
    }

    public static AddListener(func: (e: OnContactAddedEvent) => void) {
        window.addEventListener(OnContactAddedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: OnContactAddedEvent) => void) {
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

    public Dispatch(): void {
        window.dispatchEvent(this)
    }

    public static AddListener(func: (e: OnContactPersistedEvent) => void) {
        window.addEventListener(OnContactPersistedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: OnContactPersistedEvent) => void) {
        window.removeEventListener(OnContactPersistedEvent.EVENT_KEY, func as (e: Event) => void)
    }
}

export class OnContactRemovedEvent extends PhysicsEvent {
    public static readonly EVENT_KEY = "OnContactRemovedEvent"

    public message: Jolt.SubShapeIDPair

    public constructor(data: Jolt.SubShapeIDPair) {
        super(OnContactRemovedEvent.EVENT_KEY)

        this.message = data
    }

    public Dispatch(): void {
        window.dispatchEvent(this)
    }

    public static AddListener(func: (e: OnContactRemovedEvent) => void) {
        window.addEventListener(OnContactRemovedEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: OnContactRemovedEvent) => void) {
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

    public Dispatch(): void {
        window.dispatchEvent(this)
    }

    public static AddListener(func: (e: OnContactValidateEvent) => void) {
        window.addEventListener(OnContactValidateEvent.EVENT_KEY, func as (e: Event) => void)
    }

    public static RemoveListener(func: (e: OnContactValidateEvent) => void) {
        window.removeEventListener(OnContactValidateEvent.EVENT_KEY, func as (e: Event) => void)
    }
}
