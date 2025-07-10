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
} from "@/util/TypeConversions"
import * as THREE from "three"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"

class EjectableSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _gamePieceBodyId?: Jolt.BodyID

    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4
    private _ejectVelocity?: number

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

            World.physicsSystem.disablePhysicsForBody(this._gamePieceBodyId)

            // Checks if the gamepiece comes from a zone for persistent point score updates
            // because gamepieces removed by intake are not detected in the collision listener
            const zones = [...World.sceneRenderer.sceneObjects.entries()]
                .filter(x => {
                    const y = x[1] instanceof ScoringZoneSceneObject
                    return y
                })
                .map(x => x[1]) as ScoringZoneSceneObject[]

            zones.forEach(x => {
                if (this._gamePieceBodyId) ScoringZoneSceneObject.removeGamepiece(x, this._gamePieceBodyId)
            })

            console.debug("Ejectable created successfully!")
        }
    }

    public update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._gamePieceBodyId) {
            if (!World.physicsSystem.isBodyAdded(this._gamePieceBodyId)) {
                this._gamePieceBodyId = undefined
                return
            }

            // I had a think and free wrote this matrix math on a whim. It worked first try and I honestly can't quite remember how it works... -Hunter
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
