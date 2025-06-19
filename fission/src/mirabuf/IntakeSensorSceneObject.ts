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
    ThreeVector3_JoltRVec3,
} from "@/util/TypeConversions"
import { OnContactPersistedEvent } from "@/systems/physics/ContactEvents"

class IntakeSensorSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _joltBodyId?: Jolt.BodyID
    private _collision?: (e: OnContactPersistedEvent) => void
    private _visualIndicator?: THREE.Mesh

    public constructor(parentAssembly: MirabufSceneObject) {
        super()
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

            this._collision = (event: OnContactPersistedEvent) => {
                if (this._parentAssembly.intakeActive) {
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
            }

            OnContactPersistedEvent.AddListener(this._collision)
        }

        // Create visual indicator if showZoneAlways is enabled
        this.UpdateVisualIndicator()
    }

    public UpdateVisualIndicator(): void {
        // Remove existing visual indicator
        if (this._visualIndicator) {
            World.SceneRenderer.scene.remove(this._visualIndicator)
            this._visualIndicator = undefined
        }

        // Create new visual indicator if showZoneAlways is enabled
        if (this._parentAssembly.intakePreferences?.showZoneAlways) {
            const geometry = new THREE.SphereGeometry(this._parentAssembly.intakePreferences.zoneDiameter / 2.0)
            const material = new THREE.MeshBasicMaterial({
                color: 0x00ff00, // Green color for intake zone
                transparent: true,
                opacity: 0.3,
                wireframe: true,
            })
            this._visualIndicator = new THREE.Mesh(geometry, material)
            World.SceneRenderer.scene.add(this._visualIndicator)
        }
    }

    public SetVisualIndicatorVisible(visible: boolean): void {
        if (this._visualIndicator) {
            this._visualIndicator.visible = visible && (this._parentAssembly.intakePreferences?.showZoneAlways ?? false)
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

            World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltRVec3(position))
            World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

            // Update visual indicator position if it exists
            if (this._visualIndicator) {
                this._visualIndicator.position.copy(position)
                this._visualIndicator.quaternion.copy(rotation)
            }
        }
    }

    public Dispose(): void {
        if (this._joltBodyId) {
            World.PhysicsSystem.DestroyBodyIds(this._joltBodyId)
        }

        if (this._collision) OnContactPersistedEvent.RemoveListener(this._collision)

        // Clean up visual indicator
        if (this._visualIndicator) {
            World.SceneRenderer.scene.remove(this._visualIndicator)
            this._visualIndicator = undefined
        }
    }

    private IntakeCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            this._parentAssembly.SetEjectable(gpID, false)
        }
    }
}

export default IntakeSensorSceneObject
