import { Array_ThreeMatrix4, JoltMat44_ThreeMatrix4, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "./MirabufSceneObject"
import JOLT from "@/util/loading/JoltSyncLoader"
import World from "@/systems/World"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import { OnContactAddedEvent } from "@/systems/physics/ContactEvents"
import SceneObject from "@/systems/scene/SceneObject"
import TransformGizmos from "@/ui/components/TransformGizmos"


class ScoringZoneSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _gizmo?: TransformGizmos
    private _joltBodyId?: Jolt.BodyID
    private _mesh?: THREE.Mesh
    private points = 0

    public constructor(parentAssembly: MirabufSceneObject) {
        super()

        console.debug("Trying to create scoring zone...")

        this._parentAssembly = parentAssembly
    }

    public Setup(): void {
        const zones = this._parentAssembly.fieldPreferences?.scoringZones
        if (zones) {
            //TODO pluralize
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(zones[0].parentNode ?? this._parentAssembly.rootNodeId)

            if (this._parentBodyId) {

                this._deltaTransformation = Array_ThreeMatrix4(zones[0].deltaTransformation)

                this._joltBodyId = World.PhysicsSystem.CreateSensor(
                    new JOLT.BoxShapeSettings(
                        new JOLT.Vec3(1,1,1)
                    )
                )

                if (!this._joltBodyId) {
                    console.log("Failed to create scoring zone. No Jolt Body")
                    return
                }

                this._mesh = World.SceneRenderer.CreateBox(
                    new JOLT.Vec3(1,1,1),
                    World.SceneRenderer.CreateToonMaterial(0x0000ff)
                )

                World.SceneRenderer.scene.add(this._mesh)
                
                const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform())
                const gizmoTransformation = this._deltaTransformation.premultiply(fieldTransformation)

                const translation = new THREE.Vector3(0, 0, 0)
                const rotation = new THREE.Quaternion(0, 0, 0, 1)
                const scale = new THREE.Vector3(1, 1, 1)
                gizmoTransformation.decompose(translation, rotation, scale)
        
                this._mesh.position.set(translation.x, translation.y, translation.z)
                this._mesh.rotation.setFromQuaternion(rotation)
                this._mesh.scale.set(scale.x, scale.y, scale.z)

                World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltVec3(translation))
                World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

                const shapeSettings = new JOLT.BoxShapeSettings(new JOLT.Vec3(scale.x / 2, scale.y / 2, scale.z / 2))
                const shape = shapeSettings.Create()
                World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

                const collision = (event: OnContactAddedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2
        
                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body1)
                    }
                }
        
                OnContactAddedEvent.AddListener(collision)

                console.debug("Scoring zone created successfully")
            }
        }
    }

    public Update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._joltBodyId && this._mesh) {
            const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform())
            const gizmoTransformation = this._deltaTransformation.clone().premultiply(fieldTransformation)

            const translation = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            const scale = new THREE.Vector3(1, 1, 1)
            gizmoTransformation.decompose(translation, rotation, scale)
            
            this._mesh.position.set(translation.x, translation.y, translation.z)
            this._mesh.rotation.setFromQuaternion(rotation)
            this._mesh.scale.set(scale.x, scale.y, scale.z)
        
            World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltVec3(translation))
            World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

            const shapeSettings = new JOLT.BoxShapeSettings(new JOLT.Vec3(scale.x / 2, scale.y / 2, scale.z / 2))
            const shape = shapeSettings.Create()
            World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)
        } else {
            console.debug("Failed to update scoring zone")
        }
    }

    public Dispose(): void {
        console.debug("Destroying scoring zone")

        if (this._joltBodyId) {
            World.PhysicsSystem.DestroyBodyIds(this._joltBodyId)

            if (this._mesh) {
                this._mesh.geometry.dispose()
                ;(this._mesh.material as THREE.Material).dispose()
                World.SceneRenderer.scene.remove(this._mesh)
            }
        }
    }

    private ZoneCollision(gpID: Jolt.BodyID) {
        console.log(`Scoring zone collided with ${gpID.GetIndex()}`)

        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            this.points++
            console.log(`points ${this.points}`)
        }
    }
    
}
export default ScoringZoneSceneObject