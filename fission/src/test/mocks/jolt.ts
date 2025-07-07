import { vi } from "vitest"

export function createVec3Mock() {
    return {
        GetX: vi.fn(() => 0),
        GetY: vi.fn(() => 0),
        GetZ: vi.fn(() => 0),
        Add: vi.fn(function (this: any) { return this }),
        Sub: vi.fn(function (this: any) { return this }),
        Mul: vi.fn(function (this: any) { return this }),
        Div: vi.fn(function (this: any) { return this }),
        Length: vi.fn(() => 0),
        Normalize: vi.fn(function (this: any) { return this }),
        Clone: vi.fn(function () { return createVec3Mock() }),
    }
}

export function createQuatMock() {
    return {
        GetX: vi.fn(() => 0),
        GetY: vi.fn(() => 0),
        GetZ: vi.fn(() => 0),
        GetW: vi.fn(() => 1),
    }
}

export function createBodyMock() {
    return {
        GetWorldTransform: vi.fn(() => ({
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => createQuatMock()),
        })),
        GetTranslation: vi.fn(() => createVec3Mock()),
        GetQuaternion: vi.fn(() => createQuatMock()),
        GetCenterOfMassTransform: vi.fn(() => ({
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => createQuatMock()),
        })),
        GetRotation: vi.fn(() => ({
            ...createQuatMock(),
            set: vi.fn(),
            clone: vi.fn(() => ({ ...createQuatMock() })),
        })),
        GetID: vi.fn(),
        IsActive: vi.fn(),
        IsRigidBody: vi.fn(),
        IsSoftBody: vi.fn(),
        IsStatic: vi.fn(),
        IsKinematic: vi.fn(),
        IsDynamic: vi.fn(),
        CanBeKinematicOrDynamic: vi.fn(),
        GetBodyType: vi.fn(),
        GetMotionType: vi.fn(),
        SetIsSensor: vi.fn(),
        IsSensor: vi.fn(),
        SetUserData: vi.fn(),
        GetUserData: vi.fn(),
        GetLinearVelocity: vi.fn(() => createVec3Mock()),
        SetLinearVelocity: vi.fn(),
        SetAngularVelocity: vi.fn(),
        GetAngularVelocity: vi.fn(() => createVec3Mock()),
    }
} 