import SceneObject from "@/systems/scene/SceneObject"
import MirabufSceneObject from "./MirabufSceneObject"
import Jolt from "@azaleacolburn/jolt-physics"
import World from "@/systems/World"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertJoltQuatToThreeQuaternion,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
    convertThreeVector3ToJoltVec3,
    convertJoltVec3ToThreeVector3,
} from "@/util/TypeConversions"
import * as THREE from "three"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"

class EjectableSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _gamePieceBodyId?: Jolt.BodyID

    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4
    private _ejectVelocity?: number

    // Animation state
    private _isAnimating = false
    private _animationStartTime = 0
    private _animationDuration = EjectableSceneObject._defaultAnimationDuration
    private _startMatrix?: THREE.Matrix4
    private _endMatrix?: THREE.Matrix4

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

    public constructor(parentAssembly: MirabufSceneObject, gamePieceBody: Jolt.BodyID) {
        super()

        console.debug("Trying to create ejectable...")

        this._parentAssembly = parentAssembly
        this._gamePieceBodyId = gamePieceBody
    }

    public setup(): void {
        if (this._parentAssembly.ejectorPreferences && this._gamePieceBodyId) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._parentAssembly.ejectorPreferences.parentNode ?? this._parentAssembly.rootNodeId
            )

            this._deltaTransformation = convertArrayToThreeMatrix4(
                this._parentAssembly.ejectorPreferences.deltaTransformation
            )
            this._ejectVelocity = this._parentAssembly.ejectorPreferences.ejectorVelocity

            // Animation start at game piece
            const gpBody = World.physicsSystem.getBody(this._gamePieceBodyId)

            // Compute the ejectable position/rotation 
            if (this._parentBodyId && this._deltaTransformation) {
                const posToCOM = convertJoltMat44ToThreeMatrix4(gpBody.GetCenterOfMassTransform()).premultiply(
                    convertJoltMat44ToThreeMatrix4(gpBody.GetWorldTransform()).invert()
                )
                const parentBody = World.physicsSystem.getBody(this._parentBodyId)
                const bodyTransform = posToCOM
                    .invert()
                    .premultiply(
                        this._deltaTransformation
                            .clone()
                            .premultiply(convertJoltMat44ToThreeMatrix4(parentBody.GetWorldTransform()))
                    )
                // Compose start and end matrices directly
                this._startMatrix = new THREE.Matrix4().compose(
                    convertJoltVec3ToThreeVector3(gpBody.GetPosition()),
                    convertJoltQuatToThreeQuaternion(gpBody.GetRotation()),
                    new THREE.Vector3(1, 1, 1)
                )
                const endPos = new THREE.Vector3()
                const endQuat = new THREE.Quaternion()
                bodyTransform.decompose(endPos, endQuat, new THREE.Vector3(1, 1, 1))
                this._endMatrix = new THREE.Matrix4().compose(
                    endPos,
                    endQuat,
                    new THREE.Vector3(1, 1, 1)
                )
            }

            this._animationDuration = EjectableSceneObject._defaultAnimationDuration
            this._isAnimating = true
            this._animationStartTime = performance.now()

            World.physicsSystem.disablePhysicsForBody(this._gamePieceBodyId)

            // Remove from scoring zones
            const zones = [...World.sceneRenderer.sceneObjects.entries()]
                .filter(x => x[1] instanceof ScoringZoneSceneObject)
                .map(x => x[1]) as ScoringZoneSceneObject[]

            zones.forEach(x => {
                if (this._gamePieceBodyId) ScoringZoneSceneObject.removeGamepiece(x, this._gamePieceBodyId)
            })

            console.debug("Ejectable created successfully!")
        }
    }

    public update(): void {
        // Animation logic: interpolate full transform (matrix)
        if (this._isAnimating && this._gamePieceBodyId && this._startMatrix && this._endMatrix) {
            const now = performance.now()
            const elapsed = (now - this._animationStartTime) / 1000
            const t = Math.min(elapsed / this._animationDuration, 1)

            // Decompose start and end
            const startPos = new THREE.Vector3()
            const startQuat = new THREE.Quaternion()
            const startScale = new THREE.Vector3()
            this._startMatrix.decompose(startPos, startQuat, startScale)

            const endPos = new THREE.Vector3()
            const endQuat = new THREE.Quaternion()
            const endScale = new THREE.Vector3()
            this._endMatrix.decompose(endPos, endQuat, endScale)

            // Interpolate
            const pos = new THREE.Vector3().lerpVectors(startPos, endPos, t)
            const quat = new THREE.Quaternion().copy(startQuat).slerp(endQuat, t)
            const scale = new THREE.Vector3().lerpVectors(startScale, endScale, t)

            // Compose interpolated matrix
            const interpMatrix = new THREE.Matrix4().compose(pos, quat, scale)

            // Decompose to set position/rotation
            interpMatrix.decompose(pos, quat, scale)
            World.physicsSystem.setBodyPosition(this._gamePieceBodyId, convertThreeVector3ToJoltRVec3(pos), false)
            World.physicsSystem.setBodyRotation(this._gamePieceBodyId, convertThreeQuaternionToJoltQuat(quat), false)

            if (t >= 1) {
                this._isAnimating = false
            }
            return
        }

        // After animation, keep gamepiece at ejectable position
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
            const bodyTransform = posToCOM
                .invert()
                .premultiply(
                    this._deltaTransformation
                        .clone()
                        .premultiply(convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()))
                )
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
