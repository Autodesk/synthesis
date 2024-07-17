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

            if (!World.PhysicsSystem.isPaused) {
                // TEMPORARY GAME PIECE DETECTION
                // const hitRes = World.PhysicsSystem.RayCast(ThreeVector3_JoltVec3(position), new JOLT.Vec3(0, 0, 3))
                // if (hitRes) {
                //     const gpAssoc = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(hitRes.data.mBodyID)
                //     // This works, however the check for game piece is doing two checks.
                //     if (gpAssoc?.isGamePiece) {
                //         console.debug("Found game piece!")
                //         this._parentAssembly.SetEjectable(hitRes.data.mBodyID, false)
                //     }
                // }

                const collision = (e: Event) => {
                    const event = e as OnContactAddedEvent
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetID().GetIndex() == this._joltBodyId?.GetIndex()) {
                        this.IntakeCollision(body2.GetID())
                    } else if (body2.GetID().GetIndex() == this._joltBodyId?.GetIndex()) {
                        this.IntakeCollision(body1.GetID())
                    }
                }

                OnContactAddedEvent.AddListener(collision)

            }
        }
    }

    public Dispose(): void {
        console.debug("Destroying intake sensor")

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
