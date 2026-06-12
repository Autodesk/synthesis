import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import EventSystem from "@/systems/EventSystem.ts"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertArrayToThreeMatrix4, convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"

// Module-level scratch THREE objects reused each frame
const scratchBodyTransform = new THREE.Matrix4()
const scratchPosition = new THREE.Vector3()
const scratchRotation = new THREE.Quaternion()
const scratchScale = new THREE.Vector3(1, 1, 1)

class IntakeSensorSceneObject extends SceneObject {
    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _joltBodyId?: Jolt.BodyID
    private _collisionUnsubscriber?: () => void
    private _visualIndicator?: THREE.Mesh

    // Scratch WASM objects reused each frame
    private _scratchRVec3: Jolt.RVec3
    private _scratchQuat: Jolt.Quat

    public constructor(parentAssembly: MirabufSceneObject) {
        super()
        this._parentAssembly = parentAssembly
        this._scratchRVec3 = new JOLT.RVec3(0, 0, 0)
        this._scratchQuat = new JOLT.Quat(0, 0, 0, 1)
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

            this._collisionUnsubscriber = EventSystem.listen("OnContactPersistedEvent", data => {
                if (!this._parentAssembly.intakeActive || this._joltBodyId == null || World.physicsSystem.isPaused)
                    return

                const body1 = data.body1
                const body2 = data.body2

                if (body1.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                    this.intakeCollision(body2)
                } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId.GetIndexAndSequenceNumber()) {
                    this.intakeCollision(body1)
                }
            })
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
            scratchBodyTransform
                .copy(this._deltaTransformation)
                .premultiply(convertJoltMat44ToThreeMatrix4(parentBody.GetWorldTransform()))
            scratchBodyTransform.decompose(scratchPosition, scratchRotation, scratchScale)

            this._scratchRVec3.Set(scratchPosition.x, scratchPosition.y, scratchPosition.z)
            this._scratchQuat.Set(scratchRotation.x, scratchRotation.y, scratchRotation.z, scratchRotation.w)
            World.physicsSystem.setBodyPosition(this._joltBodyId, this._scratchRVec3)
            World.physicsSystem.setBodyRotation(this._joltBodyId, this._scratchQuat)

            // Update visual indicator position if it exists
            if (this._visualIndicator) {
                this._visualIndicator.position.copy(scratchPosition)
                this._visualIndicator.quaternion.copy(scratchRotation)
            }
        }
    }

    public dispose(): void {
        if (this._joltBodyId) {
            World.physicsSystem.destroyBodyIds(this._joltBodyId)
        }

        this._collisionUnsubscriber?.()

        JOLT.destroy(this._scratchRVec3)
        JOLT.destroy(this._scratchQuat)

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
