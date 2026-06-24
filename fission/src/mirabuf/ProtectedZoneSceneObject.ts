import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import EventSystem, { type SynthesisEventListener } from "@/systems/EventSystem.ts"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import ZoneSceneObject from "@/mirabuf/ZoneSceneObject"
import World from "@/systems/World"
import { MiraType } from "./MirabufLoader"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"
import { ContactType } from "./ZoneTypes"
import { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"

class ProtectedZoneSceneObject extends ZoneSceneObject<ProtectedZonePreferences> {
    private _robotsInside: Map<MirabufSceneObject, number> = new Map()

    private _lastRobotCollisionTime: number = 0

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ZoneSceneObject.darkRedMaterial, blue: ZoneSceneObject.darkBlueMaterial }
    }

    private isZoneActive(): boolean {
        if (!this.prefs?.activeDuring) {
            return [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME].includes(
                MatchMode.getInstance().getMatchModeType()
            )
        }
        return this.prefs.activeDuring.includes(MatchMode.getInstance().getMatchModeType())
    }

    private isRobotInside(robot: MirabufSceneObject): boolean {
        const timeInside = this._robotsInside.get(robot) ?? 0
        return Date.now() - timeInside < 100
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        super(parentAssembly, parentAssembly.fieldPreferences?.protectedZones[index]!, "RenderProtectedZones", render)

        this.toRender ??= PreferencesSystem.getGlobalPreference("RenderProtectedZones")
    }

    // This is used by `super.setup`
    public setupCollisionSubscribers() {
        // Detect when something enters or persists in the zone
        const collisionSubscriber: SynthesisEventListener<"OnContactAddedEvent" | "OnContactPersistedEvent"> = data => {
            const { body1, body2 } = data

            if (body1.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                this.zoneCollision(body2)
            } else if (body2.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                this.zoneCollision(body1)
            }

            // Handle contact-based penalties based on the configured contact type
            if (this.prefs?.contactType == ContactType.ROBOT_ENTERS || !this.isZoneActive()) return

            this.handleContactPenalty(body1, body2)
        }
        this.unsubscribers.push(EventSystem.listen("OnContactAddedEvent", collisionSubscriber))
        this.unsubscribers.push(EventSystem.listen("OnContactPersistedEvent", collisionSubscriber))

        // Detects when something leaves the zone
        this.unsubscribers.push(
            EventSystem.listen("OnContactRemovedEvent", ({ message }) => {
                const body1 = message.GetBody1ID()
                const body2 = message.GetBody2ID()

                const idx1 = body1.GetIndexAndSequenceNumber()
                const idx2 = body2.GetIndexAndSequenceNumber()

                const bodyIdx = this.joltBodyId?.GetIndexAndSequenceNumber()

                if (idx1 == bodyIdx) {
                    this.zoneCollisionRemoved(body2)
                } else if (idx2 == bodyIdx) {
                    this.zoneCollisionRemoved(body1)
                }
            })
        )
    }

    // NOTE for azalea
    // This function disposes of the `ProtectedZoneSceneObject` correctly
    public dispose(): void {
        if (this.joltBodyId) {
            World.physicsSystem.destroyBodyIds(this.joltBodyId)
            if (this.mesh) {
                this.mesh.geometry.dispose()
                ;(this.mesh.material as THREE.Material).dispose()
                World.sceneRenderer.scene.remove(this.mesh)
            }
        }

        this.unsubscribers.forEach(func => func())
    }

    private zoneCollision(collisionID: Jolt.BodyID) {
        if (!this.isZoneActive()) return

        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(collisionID)
        const collisionObject = associate.sceneObject as MirabufSceneObject
        if (collisionObject.miraType !== MiraType.ROBOT) return

        const entered =
            this.prefs?.contactType === ContactType.ROBOT_ENTERS &&
            collisionObject.alliance !== this.prefs?.alliance &&
            !this.isRobotInside(collisionObject)

        if (entered) {
            ScoreTracker.robotPenalty(collisionObject, this.prefs?.penaltyPoints ?? 0, `Entered protected zone`)
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
            robot => robot.alliance !== this.prefs?.alliance
        )
        if (!opposingRobot) return
        switch (this.prefs?.contactType) {
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
                this.prefs?.penaltyPoints ?? 0,
                `Contact penalty in protected zone`
            )
        }
    }
}

export default ProtectedZoneSceneObject
