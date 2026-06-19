import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertJoltQuatToThreeQuaternion,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
    convertThreeVector3ToJoltVec3,
} from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"
import JOLT from "@/util/loading/JoltSyncLoader"

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

    private readonly _scratchVec3a = new THREE.Vector3()
    private readonly _scratchVec3b = new THREE.Vector3()
    private readonly _scratchQuat = new THREE.Quaternion()
    private readonly _scratchScale = new THREE.Vector3(1, 1, 1)
    private readonly _scratchMatA = new THREE.Matrix4()
    private readonly _scratchMatB = new THREE.Matrix4()

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
            const posToCOM = convertJoltMat44ToThreeMatrix4(gpBody.GetCenterOfMassTransform()).premultiply(
                convertJoltMat44ToThreeMatrix4(gpBody.GetWorldTransform()).invert()
            )

            const body = World.physicsSystem.getBody(this._parentBodyId)

            // Compute target world transform into scratchMatA
            this._scratchMatA.copy(this._deltaTransformation).premultiply(
                convertJoltMat44ToThreeMatrix4(body.GetWorldTransform())
            )
            this._scratchMatA.decompose(this._scratchVec3a, this._scratchQuat, this._scratchScale)

            if (t < 1 && this._startTranslation && this._startRotation) {
                // gradual acceleration via easedT
                this._scratchVec3a.lerpVectors(this._startTranslation, this._scratchVec3a, easedT)
                this._scratchQuat.copy(this._startRotation).slerp(this._scratchQuat, easedT)
            }

            // apply the transform
            this._scratchMatA.identity().compose(this._scratchVec3a, this._scratchQuat, this._scratchScale)

            this._scratchMatB.copy(posToCOM).invert().premultiply(this._scratchMatA)
            this._scratchMatB.decompose(this._scratchVec3b, this._scratchQuat, this._scratchScale)
            const position = this._scratchVec3b
            const rotation = this._scratchQuat

            World.physicsSystem.setBodyPosition(this._gamePieceBodyId, convertThreeVector3ToJoltRVec3(position))
            World.physicsSystem.setBodyRotation(
                this._gamePieceBodyId,
                convertThreeQuaternionToJoltQuat(rotation),
                JOLT.EActivation_DontActivate
            )
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

        const ejectVector = convertThreeVector3ToJoltVec3(ejectDir.multiplyScalar(this._ejectVelocity))
        // NOTE
        // Don't destroy these because it seems like `gpBody` takes ownership???
        gpBody.SetLinearVelocity(parentBody.GetLinearVelocity().Add(ejectVector))
        gpBody.SetAngularVelocity(parentBody.GetAngularVelocity())

        this._parentBodyId = undefined

        JOLT.destroy(ejectVector)
    }

    public dispose(): void {
        console.debug("Destroying ejectable")

        if (this._gamePieceBodyId) {
            World.physicsSystem.enablePhysicsForBody(this._gamePieceBodyId)
        }
    }
}

export default EjectableSceneObject
