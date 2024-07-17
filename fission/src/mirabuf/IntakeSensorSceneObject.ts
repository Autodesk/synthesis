import SceneObject from "@/systems/scene/SceneObject"
import MirabufSceneObject, { RigidNodeAssociate } from "./MirabufSceneObject"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    Array_ThreeMatrix4,
    JoltMat44_ThreeMatrix4,
    ThreeQuaternion_JoltQuat,
    ThreeVector3_JoltVec3,
} from "@/util/TypeConversions"
import { OnContactAddedEvent } from "@/systems/physics/ContactEvents"

class IntakeSensorSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _joltBodyId?: Jolt.BodyID
    private _mesh?: THREE.Mesh

    public constructor(parentAssembly: MirabufSceneObject) {
        super()

        console.debug("Trying to create intake sensor...")

        this._parentAssembly = parentAssembly
    }

    public Setup(): void {
        if (this._parentAssembly.intakePreferences) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._parentAssembly.intakePreferences.parentNode ?? this._parentAssembly.rootNodeId
            )

            this._deltaTransformation = Array_ThreeMatrix4(this._parentAssembly.intakePreferences.deltaTransformation)

            this._joltBodyId = World.PhysicsSystem.CreateSensor(
                new JOLT.SphereShapeSettings(this._parentAssembly.intakePreferences.zoneDiameter / 2.0)
            )
            if (!this._joltBodyId) {
                console.error("Failed to create intake. No Jolt Body")
                return
            }

            this._mesh = World.SceneRenderer.CreateSphere(
                this._parentAssembly.intakePreferences.zoneDiameter / 2.0,
                World.SceneRenderer.CreateToonMaterial(0x5eeb67)
            )
            World.SceneRenderer.scene.add(this._mesh)

            const collision = (event: OnContactAddedEvent) => {
                //TODO: Add intake key pressed check
                if (this._joltBodyId && !World.PhysicsSystem.isPaused) {
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                        this.IntakeCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                        this.IntakeCollision(body1)
                    }
                }
            }

            OnContactAddedEvent.AddListener(collision)

            console.debug("Intake sensor created successfully!")
        }
    }

    public Update(): void {
        if (this._joltBodyId && this._parentBodyId && this._deltaTransformation) {
            const parentBody = World.PhysicsSystem.GetBody(this._parentBodyId)
            const bodyTransform = this._deltaTransformation
                .clone()
                .premultiply(JoltMat44_ThreeMatrix4(parentBody.GetWorldTransform()))
            const position = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            bodyTransform.decompose(position, rotation, new THREE.Vector3(1, 1, 1))

            World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltVec3(position))
            World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

            if (this._mesh) {
                this._mesh.position.setFromMatrixPosition(bodyTransform)
                this._mesh.rotation.setFromRotationMatrix(bodyTransform)
            }
        }
    }

    public Dispose(): void {
        console.log("Destroying intake sensor")

        if (this._joltBodyId) {
            World.PhysicsSystem.DestroyBodyIds(this._joltBodyId)

            if (this._mesh) {
                this._mesh.geometry.dispose()
                ;(this._mesh.material as THREE.Material).dispose()
                World.SceneRenderer.scene.remove(this._mesh)
            }
        }
    }

    private IntakeCollision(gpID: Jolt.BodyID) {
        console.log(`Intake collided with ${gpID.GetIndex()}`)

        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            this._parentAssembly.SetEjectable(gpID, false)
        }
    }
}

export default IntakeSensorSceneObject
