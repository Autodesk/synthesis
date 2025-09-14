import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import { OnContactAddedEvent, OnContactPersistedEvent, OnContactRemovedEvent } from "@/systems/physics/ContactEvents"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
} from "@/util/TypeConversions"
import { deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"
import { MiraType } from "./MirabufLoader"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"
import { ContactType } from "./ZoneTypes"

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
    private _collision?: (event: OnContactAddedEvent | OnContactPersistedEvent) => void
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

    private isRobotInside(robot: MirabufSceneObject): boolean {
        const timeInside = this._robotsInside.get(robot) ?? 0
        return Date.now() - timeInside < 100
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

                // Detect when something enters or persists in the zone
                this._collision = (event: OnContactAddedEvent | OnContactPersistedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2

                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollision(body1)
                    }

                    // Handle contact-based penalties based on the configured contact type
                    if (this._prefs?.contactType == ContactType.ROBOT_ENTERS || !this.isZoneActive()) return
                    this.handleContactPenalty(body1, body2)
                }
                OnContactAddedEvent.addListener(this._collision)
                OnContactPersistedEvent.addListener(this._collision)

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

        if (this._collision) {
            OnContactAddedEvent.removeListener(this._collision)
            OnContactPersistedEvent.removeListener(this._collision)
        }
        if (this._collisionRemoved) OnContactRemovedEvent.removeListener(this._collisionRemoved)
    }

    private zoneCollision(collisionID: Jolt.BodyID) {
        if (!this.isZoneActive()) return

        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        if (collisionObject.miraType !== MiraType.ROBOT) return

        if (
            this._prefs?.contactType === ContactType.ROBOT_ENTERS &&
            collisionObject.alliance !== this._prefs?.alliance &&
            !this.isRobotInside(collisionObject)
        ) {
            ScoreTracker.robotPenalty(collisionObject, this._prefs?.penaltyPoints ?? 0, `Entered protected zone`)
        }

        this._robotsInside.set(collisionObject, Date.now())
    }

    private zoneCollisionRemoved(collisionID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        this._robotsInside.set(collisionObject, Date.now())
    }

    private handleContactPenalty(body1: Jolt.BodyID, body2: Jolt.BodyID) {
        const [collisionObjectBody1, collisionObjectBody2] = [body1, body2].map(body => {
            const associate = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate | undefined
            return associate?.sceneObject as MirabufSceneObject | undefined
        })

        if (!collisionObjectBody1 || !collisionObjectBody2) return
        if (collisionObjectBody1.miraType !== MiraType.ROBOT || collisionObjectBody2.miraType !== MiraType.ROBOT) return

        // Only penalize collisions between robots from different alliances
        if (collisionObjectBody1.alliance === collisionObjectBody2.alliance) return

        // Ensures that infinite collisions do not occur
        if (Date.now() - this._lastRobotCollisionTime < 500) return

        let shouldPenalize = false

        // Find the robot that has the opposite alliance from the zone
        const opposingRobot = [collisionObjectBody1, collisionObjectBody2].find(
            robot => robot.alliance !== this._prefs?.alliance
        )
        if (!opposingRobot) return
        switch (this._prefs?.contactType) {
            case ContactType.BOTH_ROBOTS_INSIDE:
                // Penalize opposing robot if both robots are inside the zone and colliding
                if (this.isRobotInside(collisionObjectBody1) && this.isRobotInside(collisionObjectBody2)) {
                    shouldPenalize = true
                }
                break

            case ContactType.ANY_ROBOT_INSIDE:
                // Penalize if any robot is inside the zone when collision occurs
                if (this.isRobotInside(collisionObjectBody1) || this.isRobotInside(collisionObjectBody2)) {
                    shouldPenalize = true
                }
                break

            case ContactType.RED_ROBOT_INSIDE: {
                // Penalize if the red robot is inside the zone when collision occurs
                const redRobot = [collisionObjectBody1, collisionObjectBody2].find(robot => robot.alliance === "red")
                if (redRobot && this.isRobotInside(redRobot)) {
                    shouldPenalize = true
                }
                break
            }

            case ContactType.BLUE_ROBOT_INSIDE: {
                // Penalize if the blue robot is inside the zone when collision occurs
                const blueRobot = [collisionObjectBody1, collisionObjectBody2].find(robot => robot.alliance === "blue")
                if (blueRobot && this.isRobotInside(blueRobot)) {
                    shouldPenalize = true
                }
                break
            }
        }

        if (shouldPenalize) {
            this._lastRobotCollisionTime = Date.now()
            ScoreTracker.robotPenalty(
                opposingRobot,
                this._prefs?.penaltyPoints ?? 0,
                `Contact penalty in protected zone`
            )
        }
    }
}

export default ProtectedZoneSceneObject
