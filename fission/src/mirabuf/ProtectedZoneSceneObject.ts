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
import { assert } from "vitest"

type RobotBox = [MirabufSceneObject, Jolt.AABox]
type Collision = [MirabufSceneObject, MirabufSceneObject]

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
        return this._robotsInside.has(robot)
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        super(parentAssembly, parentAssembly.fieldPreferences?.protectedZones[index]!, "RenderProtectedZones")
    }

    public checkObjectsInZone(): void {
        if (!this.isZoneActive()) return

        const robots = World.sceneRenderer.mirabufSceneObjects
            .getRobots()
            .map(robot => [robot, robot.getBounding()] as RobotBox)

        const robotsInZone = robots.filter(([_robot, bounding]) => this.bounding?.OverlapsAABox(bounding))
        const oldRobotsInZone = [...this._robotsInside.keys()]

        const { added, removed } = findListDifference(
            oldRobotsInZone,
            robotsInZone.map(([robot, _]) => robot)
        )

        added.forEach(robot => this._robotsInside.set(robot, Date.now()))
        removed.forEach(robot => this._robotsInside.delete(robot))

        if (this.prefs.contactType === ContactType.ROBOT_ENTERS) {
            added.forEach(robot => this.penalizeEnteringZone(robot))
            // No reason to do any collision checking if this zone has a different `ContactType`
            return
        }

        const collisions = this.checkCollisions(robots, robotsInZone)
        collisions.forEach(robots => this.handleContactPenalty(...robots))
    }

    private checkCollisions(robots: RobotBox[], robotsInZone: RobotBox[]): Collision[] {
        const collisions: Collision[] = []

        const checkCollision = ([robot1, bounding1]: RobotBox, [robot2, bounding2]: RobotBox) => {
            if (robot1.alliance === robot2.alliance) return

            const collided = bounding1.OverlapsAABox(bounding2)
            if (!collided) return
            if (collisions.includes([robot2, robot1])) return

            collisions.push([robot1, robot2])
        }

        const checkIn = (firstList: RobotBox[], secondList: RobotBox[]) =>
            firstList.forEach(one => secondList.forEach(two => checkCollision(one, two)))

        switch (this.prefs.contactType) {
            case ContactType.BOTH_ROBOTS_INSIDE:
                // Get collisions between opposing robots inside the zone
                checkIn(robotsInZone, robotsInZone)
                break

            case ContactType.ANY_ROBOT_INSIDE:
                checkIn(robotsInZone, robots)
                break

            case ContactType.RED_ROBOT_INSIDE:
                const redRobotsInside = robotsInZone.filter(([robot, _]) => robot.alliance === "red")
                checkIn(redRobotsInside, robots)
                break

            case ContactType.BLUE_ROBOT_INSIDE:
                const blueRobotsInside = robotsInZone.filter(([robot, _]) => robot.alliance === "blue")
                checkIn(blueRobotsInside, robots)
                break
        }

        return collisions
    }

    private penalizeEnteringZone(robot: MirabufSceneObject) {
        if (robot.alliance != this.prefs.alliance)
            ScoreTracker.robotPenalty(robot, this.prefs.penaltyPoints ?? 0, "Entered Protected Zone")

        this._robotsInside.set(robot, Date.now())
    }

    /**
     * Handles collisions between two robots when either one of those robots is in this protected zone
     */
    private handleContactPenalty(body1: MirabufSceneObject, body2: MirabufSceneObject) {
        // Ensures that infinite collisions do not occur
        if (Date.now() - this._lastRobotCollisionTime < 500) return

        // Find the robot that has the opposite alliance from the zone
        const opposingRobot = [body1, body2].find(robot => robot.alliance !== this.prefs?.alliance)
        if (!opposingRobot) return

        this._lastRobotCollisionTime = Date.now()
        ScoreTracker.robotPenalty(opposingRobot, this.prefs?.penaltyPoints ?? 0, `Contact penalty in protected zone`)
    }
}

export default ProtectedZoneSceneObject
