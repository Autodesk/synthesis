import SceneObject from "@/systems/scene/SceneObject";
import MirabufSceneObject from "./MirabufSceneObject";
import Jolt from "@barclah/jolt-physics";
import World from "@/systems/World";
import { Array_ThreeMatrix4, JoltMat44_ThreeMatrix4, JoltQuat_ThreeQuaternion, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3 } from "@/util/TypeConversions";
import * as THREE from 'three';

class EjectableSceneObject extends SceneObject {

    private _parentAssembly: MirabufSceneObject
    private _gamePieceBodyId?: Jolt.BodyID

    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4
    private _ejectVelocity?: number

    public get gamePieceBodyId() {
        return this._gamePieceBodyId
    }

    public constructor(parentAssembly: MirabufSceneObject, gamePieceBody: Jolt.BodyID) {
        super()

        console.debug('Trying to create ejectable...')

        this._parentAssembly = parentAssembly
        this._gamePieceBodyId = gamePieceBody
    }

    public Setup(): void {
        if (this._parentAssembly.ejectorPreferences && this._gamePieceBodyId) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._parentAssembly.ejectorPreferences.parentNode ?? this._parentAssembly.rootNodeId
            )

            this._deltaTransformation = Array_ThreeMatrix4(this._parentAssembly.ejectorPreferences.deltaTransformation)
            this._ejectVelocity = this._parentAssembly.ejectorPreferences.ejectorVelocity

            World.PhysicsSystem.DisablePhysicsForBody(this._gamePieceBodyId)

            console.debug('Ejectable created successfully!')
        }
    }

    public Update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._gamePieceBodyId) {
            if (!World.PhysicsSystem.IsBodyAdded(this._gamePieceBodyId)) {
                this._gamePieceBodyId = undefined
                return
            }

            const gpBody = World.PhysicsSystem.GetBody(this._gamePieceBodyId)
            const posToCOM = JoltMat44_ThreeMatrix4(gpBody.GetCenterOfMassTransform()).premultiply(JoltMat44_ThreeMatrix4(gpBody.GetWorldTransform()).invert())

            const body = World.PhysicsSystem.GetBody(this._parentBodyId)
            const bodyTransform = posToCOM.invert().premultiply(this._deltaTransformation.clone().premultiply(JoltMat44_ThreeMatrix4(body.GetWorldTransform())))
            const position = new THREE.Vector3(0,0,0)
            const rotation = new THREE.Quaternion(0,0,0,1)
            bodyTransform.decompose(position, rotation, new THREE.Vector3(1,1,1))
            
            World.PhysicsSystem.SetBodyPosition(this._gamePieceBodyId, ThreeVector3_JoltVec3(position), false)
            World.PhysicsSystem.SetBodyRotation(this._gamePieceBodyId, ThreeQuaternion_JoltQuat(rotation), false)
        }
    }

    public Eject() {
        if (!this._parentBodyId || !this._ejectVelocity || !this._gamePieceBodyId) {
            return
        }

        if (!World.PhysicsSystem.IsBodyAdded(this._gamePieceBodyId)) {
            this._gamePieceBodyId = undefined
            return
        }

        const parentBody = World.PhysicsSystem.GetBody(this._parentBodyId)
        const gpBody = World.PhysicsSystem.GetBody(this._gamePieceBodyId)
        const ejectDir = new THREE.Vector3(0,0,1).applyQuaternion(JoltQuat_ThreeQuaternion(gpBody.GetRotation())).normalize()

        World.PhysicsSystem.EnablePhysicsForBody(this._gamePieceBodyId)
        gpBody.SetLinearVelocity(parentBody.GetLinearVelocity().Add(ThreeVector3_JoltVec3(ejectDir.multiplyScalar(this._ejectVelocity))))
        gpBody.SetAngularVelocity(parentBody.GetAngularVelocity())

        this._parentBodyId = undefined
    }

    public Dispose(): void {
        console.debug('Destroying ejectable')

        if (this._gamePieceBodyId) {
            World.PhysicsSystem.EnablePhysicsForBody(this._gamePieceBodyId)
        }
    }

}

export default EjectableSceneObject