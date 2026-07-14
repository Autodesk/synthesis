import type Jolt from "@synthesis.adsk/jolt-physics"

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
