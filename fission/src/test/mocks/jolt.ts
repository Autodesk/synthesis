// biome-ignore-all lint/style/useNamingConvention: Match Jolt functions
import JOLT from "@/util/loading/JoltSyncLoader"
import { vi } from "vitest"

interface Vec3Mock {
    GetX(): number
    GetY(): number
    GetZ(): number
    Add(this: Vec3Mock): Vec3Mock
    Sub(this: Vec3Mock): Vec3Mock
    Mul(this: Vec3Mock): Vec3Mock
    Div(this: Vec3Mock): Vec3Mock
    Length(): number
    Normalize(this: Vec3Mock): Vec3Mock
    Clone(): Vec3Mock
}

export function createVec3Mock(): Vec3Mock {
    return {
        GetX: vi.fn(() => 0),
        GetY: vi.fn(() => 0),
        GetZ: vi.fn(() => 0),
        Add: vi.fn(function (this: Vec3Mock) {
            return this
        }),
        Sub: vi.fn(function (this: Vec3Mock) {
            return this
        }),
        Mul: vi.fn(function (this: Vec3Mock) {
            return this
        }),
        Div: vi.fn(function (this: Vec3Mock) {
            return this
        }),
        Length: vi.fn(() => 0),
        Normalize: vi.fn(function (this: Vec3Mock) {
            return this
        }),
        Clone: vi.fn(() => createVec3Mock()),
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

export function createRotationMock() {
    return {
        ...createQuatMock(),
        set: vi.fn(),
        clone: vi.fn(() => ({ ...createQuatMock() })),
        Inversed: vi.fn(() => JOLT.Mat44.prototype.sRotation(JOLT.Quat.prototype.sIdentity())),
    }
}

export function createWorldTransform() {
    return {
        GetTranslation: vi.fn(() => createVec3Mock()),
        GetQuaternion: vi.fn(() => createQuatMock()),
        GetRotation: vi.fn(() => createRotationMock()),
        MulMat44: vi.fn(() =>
            JOLT.Mat44.prototype.sRotationTranslation(new JOLT.Quat(1, 1, 1, 1), new JOLT.Vec3(1, 1, 1))
        ),
        MulVec3: vi.fn(() => new JOLT.Vec3(1, 1, 1)),
    }
}

export function createBodyMock() {
    return {
        GetWorldTransform: vi.fn(() => createWorldTransform()),
        GetTranslation: vi.fn(() => createVec3Mock()),
        GetQuaternion: vi.fn(() => createQuatMock()),
        GetCenterOfMassTransform: vi.fn(() => ({
            GetTranslation: vi.fn(() => createVec3Mock()),
            GetQuaternion: vi.fn(() => createQuatMock()),
        })),
        GetRotation: vi.fn(() => createRotationMock()),
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
        GetWorldSpaceBounds: vi.fn(),
        GetShape: vi.fn(() => {
            const settings = new JOLT.BoxShapeSettings(new JOLT.Vec3(1, 1, 1))
            const result = settings.Create()
            const shape = result.Get()
            settings.Release()

            return shape
        }),
        GetPosition: vi.fn(() => new JOLT.Vec3(1, 1, 1)),
    }
}
