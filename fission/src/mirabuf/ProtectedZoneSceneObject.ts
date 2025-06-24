import {
    Array_ThreeMatrix4,
    JoltMat44_ThreeMatrix4,
    ThreeQuaternion_JoltQuat,
    ThreeVector3_JoltRVec3,
} from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "./MirabufSceneObject"
import JOLT from "@/util/loading/JoltSyncLoader"
import World from "@/systems/World"
import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { OnContactAddedEvent, OnContactRemovedEvent, OnContactPersistedEvent } from "@/systems/physics/ContactEvents"
import SceneObject from "@/systems/scene/SceneObject"
import { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DeltaFieldTransforms_PhysicalProp } from "@/util/threejs/MeshCreation"
import { MiraType } from "./MirabufLoader"
import SimulationSystem from "@/systems/simulation/SimulationSystem"

class ProtectedZoneSceneObject extends SceneObject {
    // Colors
    static redMaterial = new THREE.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    static blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    }) //0x0000ff
    static transparentMaterial = new THREE.MeshPhongMaterial({
        color: 0x0000,
        shininess: 0.0,
        opacity: 0.0,
        transparent: true,
    })

    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _toRender: boolean
    private _prefs?: ProtectedZonePreferences
    private _joltBodyId?: Jolt.BodyID
    private _mesh?: THREE.Mesh
    private _collision?: (event: OnContactAddedEvent) => void
    private _collisionPersisted?: (event: OnContactPersistedEvent) => void
    private _collisionRemoved?: (event: OnContactRemovedEvent) => void

    private _robotsInside: Map<MirabufSceneObject, number> = new Map()

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        super()

        this._parentAssembly = parentAssembly
        this._prefs = this._parentAssembly.fieldPreferences?.protectedZones[index]
        this._toRender = render ?? PreferencesSystem.getGlobalPreference<boolean>("RenderProtectedZones")
    }

    public Setup(): void {
        if (this._prefs) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._prefs.parentNode ?? this._parentAssembly.rootNodeId
            )

            if (this._parentBodyId) {
                // Create a default sensor
                this._joltBodyId = World.PhysicsSystem.CreateSensor(new JOLT.BoxShapeSettings(new JOLT.Vec3(1, 1, 1)))
                if (!this._joltBodyId) {
                    console.log("Failed to create protected zone. No Jolt Body")
                    return
                }

                // Position/rotate/scale sensor to settings
                this._deltaTransformation = Array_ThreeMatrix4(this._prefs.deltaTransformation)
                const fieldTransformation = JoltMat44_ThreeMatrix4(
                    World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform()
                )
                const props = DeltaFieldTransforms_PhysicalProp(this._deltaTransformation, fieldTransformation)

                World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltRVec3(props.translation))
                World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(props.rotation))
                const shapeSettings = new JOLT.BoxShapeSettings(
                    new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
                )
                const shape = shapeSettings.Create()
                World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

                // Mesh for the user to visualize sensor
                this._mesh = World.SceneRenderer.CreateBox(
                    new JOLT.Vec3(1, 1, 1),
                    ProtectedZoneSceneObject.transparentMaterial
                )
                World.SceneRenderer.scene.add(this._mesh)

                if (this._toRender) {
                    this._mesh?.position.set(props.translation.x, props.translation.y, props.translation.z)
                    this._mesh?.rotation.setFromQuaternion(props.rotation)
                    this._mesh?.scale.set(props.scale.x, props.scale.y, props.scale.z)
                }

                // Detect when something enters the zone
                this._collision = (event: OnContactAddedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body1)
                    }

                    // If the preference is set to require robot contact, we want to penalize robots here
                    if (!this._prefs?.requireRobotContact) return
                    const [collisionObjectBody1, collisionObjectBody2] = [body1, body2].map(body => {
                        const associate = World.PhysicsSystem.GetBodyAssociation(body) as RigidNodeAssociate | undefined
                        return associate?.sceneObject as MirabufSceneObject | undefined
                    })
                    if (!collisionObjectBody1 || !collisionObjectBody2) return
                    // Makes sure that both robots are from opposing alliances
                    if (collisionObjectBody1.alliance === collisionObjectBody2.alliance) return
                    // Ensure that both bodies are robots are inside the zone
                    if (
                        (this._robotsInside.get(collisionObjectBody1) ?? 0 - Date.now() > 500) &&
                        (this._robotsInside.get(collisionObjectBody2) ?? 0 - Date.now() > 500)
                    ) {
                        // Penalize the robot that entered the opposing alliance protected zone
                        if (collisionObjectBody1.alliance === this._prefs?.alliance) {
                            SimulationSystem.RobotPenalty(
                                collisionObjectBody2,
                                this._prefs?.penaltyPoints ?? 0,
                                `Entered protected zone`
                            )
                        } else {
                            SimulationSystem.RobotPenalty(
                                collisionObjectBody1,
                                this._prefs?.penaltyPoints ?? 0,
                                `Entered protected zone`
                            )
                        }
                    }
                }
                OnContactAddedEvent.AddListener(this._collision)

                // Detects when something persists in the zone
                this._collisionPersisted = (event: OnContactPersistedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body1)
                    }
                }
                OnContactPersistedEvent.AddListener(this._collisionPersisted)

                // Detects when something leaves the zone
                this._collisionRemoved = (event: OnContactRemovedEvent) => {
                    const body1 = event.message.GetBody1ID()
                    const body2 = event.message.GetBody2ID()

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollisionRemoved(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollisionRemoved(body1)
                    }
                }
                OnContactRemovedEvent.AddListener(this._collisionRemoved)
            }
        }
    }

    public Update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._joltBodyId && this._prefs) {
            // Update translation, rotation, and scale
            const fieldTransformation = JoltMat44_ThreeMatrix4(
                World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform()
            )
            const props = DeltaFieldTransforms_PhysicalProp(this._deltaTransformation, fieldTransformation)

            World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltRVec3(props.translation))
            World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(props.rotation))
            const shapeSettings = new JOLT.BoxShapeSettings(
                new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
            )
            const shape = shapeSettings.Create()
            World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

            // Mesh for visualization
            this._toRender = PreferencesSystem.getGlobalPreference<boolean>("RenderProtectedZones")
            if (this._mesh)
                if (this._toRender) {
                    this._mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
                    this._mesh.rotation.setFromQuaternion(props.rotation)
                    this._mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)
                    this._mesh.material =
                        this._prefs.alliance == "red"
                            ? ProtectedZoneSceneObject.redMaterial
                            : ProtectedZoneSceneObject.blueMaterial
                } else {
                    this._mesh.material = ProtectedZoneSceneObject.transparentMaterial
                }
        }
    }

    public Dispose(): void {
        if (this._joltBodyId) {
            World.PhysicsSystem.DestroyBodyIds(this._joltBodyId)
            if (this._mesh) {
                this._mesh.geometry.dispose()
                ;(this._mesh.material as THREE.Material).dispose()
                World.SceneRenderer.scene.remove(this._mesh)
            }
        }

        if (this._collision) OnContactAddedEvent.RemoveListener(this._collision)
        if (this._collisionRemoved) OnContactRemovedEvent.RemoveListener(this._collisionRemoved)
    }

    private ZoneCollision(collisionID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        if (collisionObject.miraType === MiraType.ROBOT && collisionObject.alliance !== this._prefs?.alliance) {
            const timeInside = this._robotsInside.get(collisionObject) ?? 0
            if (!this._prefs?.requireRobotContact && Date.now() - timeInside > 500) {
                SimulationSystem.RobotPenalty(
                    collisionObject,
                    this._prefs?.penaltyPoints ?? 0,
                    `Entered protected zone`
                )
            }
            this._robotsInside.set(collisionObject, Date.now())
        }
    }

    private ZoneCollisionRemoved(collisionID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        this._robotsInside.set(collisionObject, Date.now())
    }
}

export default ProtectedZoneSceneObject
