import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { OnContactPersistedEvent } from "@/systems/physics/ContactEvents"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
} from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"

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

    public setup(): void {
        if (this._parentAssembly.intakePreferences) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._parentAssembly.intakePreferences.parentNode ?? this._parentAssembly.rootNodeId
            )

            this._deltaTransformation = convertArrayToThreeMatrix4(
                this._parentAssembly.intakePreferences.deltaTransformation
            )

            this._joltBodyId = World.physicsSystem.createSensor(
                new JOLT.SphereShapeSettings(this._parentAssembly.intakePreferences.zoneDiameter / 2.0)
            )
            if (!this._joltBodyId) {
                console.error("Failed to create intake. No Jolt Body")
                return
            }

            this._collision = (event: OnContactPersistedEvent) => {
                if (this._parentAssembly.intakeActive) {
                    if (this._joltBodyId && !World.physicsSystem.isPaused) {
                        const body1 = event.message.body1
                        const body2 = event.message.body2

                        if (body1.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                            this.intakeCollision(body2)
                        } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                            this.intakeCollision(body1)
                        }
                    }
                }
            }

            OnContactPersistedEvent.addListener(this._collision)
        }

        // Create visual indicator if showZoneAlways is enabled
        this.updateVisualIndicator()
    }

    public updateVisualIndicator(): void {
        // Remove existing visual indicator
        if (this._visualIndicator) {
            World.sceneRenderer.scene.remove(this._visualIndicator)
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
            World.sceneRenderer.scene.add(this._visualIndicator)
        }
    }

    public setVisualIndicatorVisible(visible: boolean): void {
        if (this._visualIndicator) {
            this._visualIndicator.visible = visible && (this._parentAssembly.intakePreferences?.showZoneAlways ?? false)
        }
    }

    public update(): void {
        if (this._joltBodyId && this._parentBodyId && this._deltaTransformation) {
            const parentBody = World.physicsSystem.getBody(this._parentBodyId)
            const bodyTransform = this._deltaTransformation
                .clone()
                .premultiply(convertJoltMat44ToThreeMatrix4(parentBody.GetWorldTransform()))
            const position = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            bodyTransform.decompose(position, rotation, new THREE.Vector3(1, 1, 1))

            World.physicsSystem.setBodyPosition(this._joltBodyId, convertThreeVector3ToJoltRVec3(position))
            World.physicsSystem.setBodyRotation(this._joltBodyId, convertThreeQuaternionToJoltQuat(rotation))

            // Update visual indicator position if it exists
            if (this._visualIndicator) {
                this._visualIndicator.position.copy(position)
                this._visualIndicator.quaternion.copy(rotation)
            }
        }
    }

    public dispose(): void {
        if (this._joltBodyId) {
            World.physicsSystem.destroyBodyIds(this._joltBodyId)
        }

        if (this._collision) OnContactPersistedEvent.removeListener(this._collision)

        // Clean up visual indicator
        if (this._visualIndicator) {
            World.sceneRenderer.scene.remove(this._visualIndicator)
            this._visualIndicator = undefined
        }
    }

    private intakeCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            associate.robotLastInContactWith = this._parentAssembly
            this._parentAssembly.setEjectable(gpID)
        }
    }
}

export default IntakeSensorSceneObject
