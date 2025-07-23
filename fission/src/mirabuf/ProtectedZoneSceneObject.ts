import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
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
import { deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"
import { MiraType } from "./MirabufLoader"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode, { MatchModeType } from "@/systems/MatchMode"

class ProtectedZoneSceneObject extends SceneObject {
    // Colors
    public static redMaterial = new THREE.MeshPhongMaterial({
        color: 0xff0000,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })
    public static blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0022ff,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })
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

    private _lastRobotCollisionTime: number = 0

    private isZoneActive(): boolean {
        if (!this._prefs?.activeDuring) {
            return [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME].includes(
                MatchMode.getInstance().getMatchModeType()
            )
        }
        return this._prefs.activeDuring.includes(MatchMode.getInstance().getMatchModeType())
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        super()

        this._parentAssembly = parentAssembly
        this._prefs = this._parentAssembly.fieldPreferences?.protectedZones[index]
        this._toRender = render ?? PreferencesSystem.getGlobalPreference("RenderProtectedZones")
    }

    public setup(): void {
        if (this._prefs) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._prefs.parentNode ?? this._parentAssembly.rootNodeId
            )

            if (this._parentBodyId) {
                // Create a default sensor
                this._joltBodyId = World.physicsSystem.createSensor(new JOLT.BoxShapeSettings(new JOLT.Vec3(1, 1, 1)))
                if (!this._joltBodyId) {
                    console.log("Failed to create protected zone. No Jolt Body")
                    return
                }

                // Position/rotate/scale sensor to settings
                this._deltaTransformation = convertArrayToThreeMatrix4(this._prefs.deltaTransformation)
                const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(this._parentBodyId).GetWorldTransform()
                )
                const props = deltaFieldTransformsPhysicalProp(this._deltaTransformation, fieldTransformation)

                World.physicsSystem.setBodyPosition(this._joltBodyId, convertThreeVector3ToJoltRVec3(props.translation))
                World.physicsSystem.setBodyRotation(this._joltBodyId, convertThreeQuaternionToJoltQuat(props.rotation))
                const shapeSettings = new JOLT.BoxShapeSettings(
                    new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
                )
                const shape = shapeSettings.Create()
                World.physicsSystem.setShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

                // Mesh for the user to visualize sensor
                this._mesh = World.sceneRenderer.createBox(
                    new JOLT.Vec3(1, 1, 1),
                    ProtectedZoneSceneObject.transparentMaterial
                )
                World.sceneRenderer.scene.add(this._mesh)

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
                        this.zoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollision(body1)
                    }

                    // If the preference is set to require robot contact, we want to penalize robots here
                    if (!this._prefs?.requireRobotContact || !this.isZoneActive()) return
                    const [collisionObjectBody1, collisionObjectBody2] = [body1, body2].map(body => {
                        const associate = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate | undefined
                        return associate?.sceneObject as MirabufSceneObject | undefined
                    })
                    if (!collisionObjectBody1 || !collisionObjectBody2) return
                    // Makes sure that both robots are from opposing alliances
                    if (collisionObjectBody1.alliance === collisionObjectBody2.alliance) return
                    // Ensure that both bodies are robots are inside the zone
                    if (
                        Date.now() - (this._robotsInside.get(collisionObjectBody1) ?? 0) > 500 ||
                        Date.now() - (this._robotsInside.get(collisionObjectBody2) ?? 0) > 500
                    ) {
                        return
                    }
                    // Ensures that infinite collisions do not occur
                    if (Date.now() - this._lastRobotCollisionTime < 1000) return
                    this._lastRobotCollisionTime = Date.now()

                    // Penalize the robot that entered the opposing alliance protected zone
                    if (collisionObjectBody1.alliance === this._prefs?.alliance) {
                        SimulationSystem.robotPenalty(
                            collisionObjectBody2,
                            this._prefs?.penaltyPoints ?? 0,
                            `Touched robot in protected zone`
                        )
                    } else {
                        SimulationSystem.robotPenalty(
                            collisionObjectBody1,
                            this._prefs?.penaltyPoints ?? 0,
                            `Touched robot in protected zone`
                        )
                    }
                }
                OnContactAddedEvent.addListener(this._collision)

                // Detects when something persists in the zone
                this._collisionPersisted = (event: OnContactPersistedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollision(body1)
                    }
                }
                OnContactPersistedEvent.addListener(this._collisionPersisted)

                // Detects when something leaves the zone
                this._collisionRemoved = (event: OnContactRemovedEvent) => {
                    const body1 = event.message.GetBody1ID()
                    const body2 = event.message.GetBody2ID()

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollisionRemoved(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollisionRemoved(body1)
                    }
                }
                OnContactRemovedEvent.addListener(this._collisionRemoved)
            }
        }
    }

    public update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._joltBodyId && this._prefs) {
            // Update translation, rotation, and scale
            const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(this._parentBodyId).GetWorldTransform()
            )
            const props = deltaFieldTransformsPhysicalProp(this._deltaTransformation, fieldTransformation)

            World.physicsSystem.setBodyPosition(this._joltBodyId, convertThreeVector3ToJoltRVec3(props.translation))
            World.physicsSystem.setBodyRotation(this._joltBodyId, convertThreeQuaternionToJoltQuat(props.rotation))
            const shapeSettings = new JOLT.BoxShapeSettings(
                new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
            )
            const shape = shapeSettings.Create()
            World.physicsSystem.setShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

            // Mesh for visualization
            this._toRender = PreferencesSystem.getGlobalPreference("RenderProtectedZones")
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

    public dispose(): void {
        if (this._joltBodyId) {
            World.physicsSystem.destroyBodyIds(this._joltBodyId)
            if (this._mesh) {
                this._mesh.geometry.dispose()
                ;(this._mesh.material as THREE.Material).dispose()
                World.sceneRenderer.scene.remove(this._mesh)
            }
        }

        if (this._collision) OnContactAddedEvent.removeListener(this._collision)
        if (this._collisionRemoved) OnContactRemovedEvent.removeListener(this._collisionRemoved)
    }

    private zoneCollision(collisionID: Jolt.BodyID) {
        if (!this.isZoneActive()) return

        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        if (collisionObject.miraType === MiraType.ROBOT && collisionObject.alliance !== this._prefs?.alliance) {
            const timeInside = this._robotsInside.get(collisionObject) ?? 0
            if (!this._prefs?.requireRobotContact && Date.now() - timeInside > 500) {
                SimulationSystem.robotPenalty(
                    collisionObject,
                    this._prefs?.penaltyPoints ?? 0,
                    `Entered protected zone`
                )
            }
            this._robotsInside.set(collisionObject, Date.now())
        }
    }

    private zoneCollisionRemoved(collisionID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        this._robotsInside.set(collisionObject, Date.now())
    }
}

export default ProtectedZoneSceneObject
