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
            let desiredPosition = new THREE.Vector3(0, 0, 0)
            let desiredRotation = new THREE.Quaternion(0, 0, 0, 1)

            // Compute target world transform
            const desiredTransform = this._deltaTransformation
                .clone()
                .premultiply(convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()))

            desiredTransform.decompose(desiredPosition, desiredRotation, new THREE.Vector3(1, 1, 1))

            if (t < 1 && this._startTranslation && this._startRotation) {
                // gradual acceleration via easedT
                desiredPosition = new THREE.Vector3().lerpVectors(this._startTranslation, desiredPosition, easedT)
                desiredRotation = new THREE.Quaternion().copy(this._startRotation).slerp(desiredRotation, easedT)
            }
            // } else if (t >= 1) {
            //     // snap instantly and re-enable physics
            //     World.physicsSystem.enablePhysicsForBody(this._gamePieceBodyId)
            // }

            // apply the transform
            desiredTransform.identity().compose(desiredPosition, desiredRotation, new THREE.Vector3(1, 1, 1))

            const bodyTransform = posToCOM.clone().invert().premultiply(desiredTransform)

            const position = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            bodyTransform.decompose(position, rotation, new THREE.Vector3(1, 1, 1))

            World.physicsSystem.setBodyPosition(this._gamePieceBodyId, convertThreeVector3ToJoltRVec3(position), false)
            World.physicsSystem.setBodyRotation(
                this._gamePieceBodyId,
                convertThreeQuaternionToJoltQuat(rotation),
                false
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
    }
}

export default EjectableSceneObject
