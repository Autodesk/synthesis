import * as THREE from "three"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import ZoneSceneObject from "@/mirabuf/ZoneSceneObject"
import World from "@/systems/World"
import { MiraType } from "./MirabufLoader"
import type MirabufSceneObject from "./MirabufSceneObject"
import { ContactType } from "./ZoneTypes"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import MatchMode from "@/systems/match_mode/MatchMode"
import Jolt from "@azaleacolburn/jolt-physics"
import { findListDifference } from "@/util/Utility"

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

    // private isRobotInside(robot: MirabufSceneObject): boolean {
    //     const timeInside = this._robotsInside.get(robot) ?? 0
    //     return Date.now() - timeInside < 100
    // }
    private isRobotInside(robot: MirabufSceneObject): boolean {
        return this._robotsInside.has(robot)
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        super(parentAssembly, parentAssembly.fieldPreferences?.protectedZones[index]!, "RenderProtectedZones")
    }

    public checkObjectsInZone(): void {
        if (!this.isZoneActive()) return

        const robots = World.sceneRenderer.mirabufSceneObjects.getRobots()
        const robotsInZone = robots
            .map(robot => [robot, robot.getBounding()] as [MirabufSceneObject, Jolt.AABox])
            .filter(([_robot, bounding]) => this.bounding?.OverlapsAABox(bounding))

        const oldRobotsInZone = [...this._robotsInside.keys()]

        const { added, removed } = findListDifference(
            oldRobotsInZone,
            robotsInZone.map(([robot, _]) => robot)
        )

        if (this.prefs.contactType === ContactType.ROBOT_ENTERS) {
            added.forEach(this.penalizeEnteringZone)
        }

        removed.forEach(this._robotsInside.delete)

        // Collisions of two robots within the scoring zone
        const collisions: [MirabufSceneObject, MirabufSceneObject][] = []
        for (const [robot1, bounding1] of robotsInZone) {
            for (const [robot2, bounding2] of robotsInZone) {
                const collided = bounding1.OverlapsAABox(bounding2)

                if (!collided) continue
                if (collisions.includes([robot2, robot1])) continue

                collisions.push([robot1, robot2])
            }
        }

        collisions.forEach(robots => this.handleContactPenalty(...robots))
    }

    private penalizeEnteringZone(robot: MirabufSceneObject) {
        if (robot.alliance == this.prefs.alliance)
            ScoreTracker.robotPenalty(robot, this.prefs.penaltyPoints ?? 0, "Entered Protected Zone")

        this._robotsInside.set(robot, Date.now())
    }

    private handleContactPenalty(body1: MirabufSceneObject, body2: MirabufSceneObject) {
        if (!body1 || !body2) return

        if (body1.miraType !== MiraType.ROBOT || body2.miraType !== MiraType.ROBOT) return
        // Only penalize collisions between robots from different alliances
        if (body1.alliance === body2.alliance) return

        // Ensures that infinite collisions do not occur
        if (Date.now() - this._lastRobotCollisionTime < 500) return

        let shouldPenalize = false

        // Find the robot that has the opposite alliance from the zone
        const opposingRobot = [body1, body2].find(robot => robot.alliance !== this.prefs?.alliance)
        if (!opposingRobot) return
        switch (this.prefs?.contactType) {
            case ContactType.BOTH_ROBOTS_INSIDE:
                // Penalize opposing robot if both robots are inside the zone and colliding
                if (this.isRobotInside(body1) && this.isRobotInside(body2)) {
                    shouldPenalize = true
                }
                break

            case ContactType.ANY_ROBOT_INSIDE:
                // Penalize if any robot is inside the zone when collision occurs
                if (this.isRobotInside(body1) || this.isRobotInside(body2)) {
                    shouldPenalize = true
                }
                break

            case ContactType.RED_ROBOT_INSIDE: {
                // Penalize if the red robot is inside the zone when collision occurs
                const redRobot = [body1, body2].find(robot => robot.alliance === "red")
                if (redRobot && this.isRobotInside(redRobot)) {
                    shouldPenalize = true
                }
                break
            }

            case ContactType.BLUE_ROBOT_INSIDE: {
                // Penalize if the blue robot is inside the zone when collision occurs
                const blueRobot = [body1, body2].find(robot => robot.alliance === "blue")
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
