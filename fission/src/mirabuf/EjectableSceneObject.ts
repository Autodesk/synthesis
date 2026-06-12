import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertJoltQuatToThreeQuaternion,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"

// Module-level scratch THREE objects — reused every frame to avoid GC pressure.
// Use separate matrices for values that are alive simultaneously.
const scratchMatPosToCOM = new THREE.Matrix4()
const scratchMatDesiredTransform = new THREE.Matrix4()
const scratchMatBodyTransform = new THREE.Matrix4()
const scratchMatInvWorld = new THREE.Matrix4()
const scratchDesiredPos = new THREE.Vector3()
const scratchDesiredRot = new THREE.Quaternion()
const scratchPosition = new THREE.Vector3()
const scratchRotation = new THREE.Quaternion()
const scratchScale = new THREE.Vector3(1, 1, 1)

class EjectableSceneObject extends SceneObject {
    private _parentSceneObject: MirabufSceneObject
    private _gamePieceBodyId?: Jolt.BodyID

    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4
    private _ejectVelocity?: number

    // Animation state
    private _animationStartTime = 0
    private _animationDuration = EjectableSceneObject._defaultAnimationDuration
    private _startTranslation?: THREE.Vector3
    private _startRotation?: THREE.Quaternion

    private static _defaultAnimationDuration = 0.5

    // Scratch WASM objects reused each frame
    private _scratchRVec3: Jolt.RVec3
    private _scratchQuat: Jolt.Quat

    public static setAnimationDuration(duration: number) {
        EjectableSceneObject._defaultAnimationDuration = duration
    }
    public static getAnimationDuration() {
        return EjectableSceneObject._defaultAnimationDuration
    }

    public get gamePieceBodyId() {
        return this._gamePieceBodyId
    }

    public get parentBodyId() {
        return this._parentBodyId
    }

    public get parentSceneObject(): MirabufSceneObject {
        return this._parentSceneObject
    }

    public constructor(parentAssembly: MirabufSceneObject, gamePieceBody: Jolt.BodyID) {
        super()

        console.debug("Trying to create ejectable...")

        this._parentSceneObject = parentAssembly
        this._gamePieceBodyId = gamePieceBody

        this._scratchRVec3 = new JOLT.RVec3(0, 0, 0)
        this._scratchQuat = new JOLT.Quat(0, 0, 0, 1)
    }

    public setup(): void {
        if (this._parentSceneObject.ejectorPreferences && this._gamePieceBodyId) {
            this._parentBodyId = this._parentSceneObject.mechanism.nodeToBody.get(
                this._parentSceneObject.ejectorPreferences.parentNode ?? this._parentSceneObject.rootNodeId
            )

            this._deltaTransformation = convertArrayToThreeMatrix4(
                this._parentSceneObject.ejectorPreferences.deltaTransformation
            )
            this._ejectVelocity = this._parentSceneObject.ejectorPreferences.ejectorVelocity

            // Record start transform at the game piece center of mass
            const gpBody = World.physicsSystem.getBody(this._gamePieceBodyId)
            this._startTranslation = new THREE.Vector3(0, 0, 0)
            this._startRotation = new THREE.Quaternion(0, 0, 0, 1)
            convertJoltMat44ToThreeMatrix4(gpBody.GetCenterOfMassTransform()).decompose(
                this._startTranslation,
                this._startRotation,
                new THREE.Vector3(1, 1, 1)
            )

            this._animationDuration = EjectableSceneObject._defaultAnimationDuration
            this._animationStartTime = performance.now()

            World.physicsSystem.disablePhysicsForBody(this._gamePieceBodyId)

            // Remove from any scoring zones
            const zones = World.sceneRenderer.filterSceneObjects(x => x instanceof ScoringZoneSceneObject)
            zones.forEach(x => {
                if (this._gamePieceBodyId) ScoringZoneSceneObject.removeGamepiece(x, this._gamePieceBodyId)
            })

            console.debug("Ejectable created successfully!")
        }
    }

    public update(): void {
        const now = performance.now()
        const elapsed = (now - this._animationStartTime) / 1000
        const tRaw = elapsed / this._animationDuration
        const t = Math.min(tRaw, 1)

        // ease-in curve for gradual acceleration
        const easedT = t * t

        if (this._parentBodyId && this._deltaTransformation && this._gamePieceBodyId) {
            if (!World.physicsSystem.isBodyAdded(this._gamePieceBodyId)) {
                this._gamePieceBodyId = undefined
                return
            }

            const gpBody = World.physicsSystem.getBody(this._gamePieceBodyId)
            // posToCOM = CenterOfMassTransform * inverse(WorldTransform)
            scratchMatInvWorld.copy(convertJoltMat44ToThreeMatrix4(gpBody.GetWorldTransform())).invert()
            scratchMatPosToCOM.copy(convertJoltMat44ToThreeMatrix4(gpBody.GetCenterOfMassTransform()))
            scratchMatPosToCOM.premultiply(scratchMatInvWorld)

            const body = World.physicsSystem.getBody(this._parentBodyId)

            // Compute target world transform
            scratchMatDesiredTransform
                .copy(this._deltaTransformation)
                .premultiply(convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()))

            scratchMatDesiredTransform.decompose(scratchDesiredPos, scratchDesiredRot, scratchScale)

            if (t < 1 && this._startTranslation && this._startRotation) {
                // gradual acceleration via easedT
                scratchDesiredPos.lerpVectors(this._startTranslation, scratchDesiredPos, easedT)
                scratchDesiredRot.copy(this._startRotation).slerp(scratchDesiredRot, easedT)
            }
            // } else if (t >= 1) {
            //     // snap instantly and re-enable physics
            //     World.physicsSystem.enablePhysicsForBody(this._gamePieceBodyId)
            // }

            // apply the transform
            scratchMatDesiredTransform.identity().compose(scratchDesiredPos, scratchDesiredRot, scratchScale)

            // bodyTransform = inverse(posToCOM) * desiredTransform
            scratchMatBodyTransform.copy(scratchMatPosToCOM).invert().premultiply(scratchMatDesiredTransform)

            scratchMatBodyTransform.decompose(scratchPosition, scratchRotation, scratchScale)

            this._scratchRVec3.Set(scratchPosition.x, scratchPosition.y, scratchPosition.z)
            this._scratchQuat.Set(scratchRotation.x, scratchRotation.y, scratchRotation.z, scratchRotation.w)
            World.physicsSystem.setBodyPosition(this._gamePieceBodyId, this._scratchRVec3, false)
            World.physicsSystem.setBodyRotation(this._gamePieceBodyId, this._scratchQuat, false)
        }
    }

    public eject() {
        if (!this._parentBodyId || !this._ejectVelocity || !this._gamePieceBodyId) {
            return
        }

        if (!World.physicsSystem.isBodyAdded(this._gamePieceBodyId)) {
            this._gamePieceBodyId = undefined
            return
        }

        const parentBody = World.physicsSystem.getBody(this._parentBodyId)
        const gpBody = World.physicsSystem.getBody(this._gamePieceBodyId)
        const ejectDir = new THREE.Vector3(0, 0, 1)
            .applyQuaternion(convertJoltQuatToThreeQuaternion(gpBody.GetRotation()))
            .normalize()

        World.physicsSystem.enablePhysicsForBody(this._gamePieceBodyId)
        gpBody.SetLinearVelocity(
            parentBody
                .GetLinearVelocity()
                .Add(convertThreeVector3ToJoltVec3(ejectDir.multiplyScalar(this._ejectVelocity)))
        )
        gpBody.SetAngularVelocity(parentBody.GetAngularVelocity())

        this._parentBodyId = undefined
    }

    public dispose(): void {
        console.debug("Destroying ejectable")

        if (this._gamePieceBodyId) {
            World.physicsSystem.enablePhysicsForBody(this._gamePieceBodyId)
        }

        JOLT.destroy(this._scratchRVec3)
        JOLT.destroy(this._scratchQuat)
    }
}

export default EjectableSceneObject
