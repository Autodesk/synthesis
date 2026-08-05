import type Jolt from "@synthesis.adsk/jolt-physics"
import type * as THREE from "three"
import * as Three from "three"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ZoneSceneObject from "@/mirabuf/ZoneSceneObject"
import World from "@/systems/World"
import type MirabufSceneObject from "./MirabufSceneObject"
import { ContactType } from "./ZoneTypes"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import MatchMode from "@/systems/match_mode/MatchMode"
import { findListDifference, forPair } from "@/util/Utility"
import JOLT from "@/util/loading/JoltSyncLoader"
import { renderOrientedBox } from "@/util/Render"

const DEBUG_BOUNDING_BOXES = false

type RobotBox = [MirabufSceneObject, Jolt.OrientedBox]
type Collision = [MirabufSceneObject, MirabufSceneObject]

class ProtectedZoneSceneObject extends ZoneSceneObject<ProtectedZonePreferences> {
    public static readonly RED_MATERIAL = new Three.MeshPhongMaterial({
        color: 0xff0000,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })
    public static readonly BLUE_MATERIAL = new Three.MeshPhongMaterial({
        color: 0x0022ff,
        shininess: 0.0,
        opacity: 0.8,
        transparent: true,
    })

    private _robotsInside: Map<MirabufSceneObject, number> = new Map()
    private _lastRobotCollisionTime: number = 0

    private _robotBounding: THREE.Mesh[] = []

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ProtectedZoneSceneObject.RED_MATERIAL, blue: ProtectedZoneSceneObject.BLUE_MATERIAL }
    }

    private isZoneActive(): boolean {
        if (!this.prefs?.activeDuring) {
            const type = MatchMode.getInstance().getMatchModeType()
            return [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME].includes(type)
        }
        return this.prefs.activeDuring.includes(MatchMode.getInstance().getMatchModeType())
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        super(parentAssembly, parentAssembly.fieldPreferences?.protectedZones[index]!, "RenderProtectedZones")
    }

    public override checkObjectsInZone(): void {
        if (!this.isZoneActive()) return

        const robots = World.sceneRenderer.mirabufSceneObjects
            .getRobots()
            .map(robot => [robot, robot.getOrientedBoundingBox()] as RobotBox)

        if (DEBUG_BOUNDING_BOXES) {
            this.disposeOfRobotBoundingMeshes()
            this._robotBounding = robots.map(([_, b]) => renderOrientedBox(b))
        }

        const robotsInZone = robots.filter(([_, bounding]) => this.bounding?.OverlapsOrientedBox(bounding))
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
            robots.forEach(([_, bounding]) => JOLT.destroy(bounding))
            return
        }

        const collisions = this.checkCollisions(robots, robotsInZone)
        collisions.forEach(collidedRobots => this.handleContactPenalty(...collidedRobots))

        robots.forEach(([_, bounding]) => JOLT.destroy(bounding))
    }

    private checkCollisions(robots: RobotBox[], robotsInZone: RobotBox[]): Collision[] {
        const collisions: Collision[] = []

        const isDuplicateCollision = (robot1: MirabufSceneObject, robot2: MirabufSceneObject): boolean =>
            collisions.some(collision => collision[0] === robot2 && collision[1] === robot1)

        const checkCollision = ([robot1, bounding1]: RobotBox, [robot2, bounding2]: RobotBox) => {
            if (robot1.alliance === robot2.alliance) return
            if (isDuplicateCollision(robot1, robot2)) return

            const collided = bounding1.OverlapsOrientedBox(bounding2)
            if (!collided) return

            collisions.push([robot1, robot2])
        }

        const checkIn = (first: RobotBox[], second: RobotBox[]) => forPair(first, second, checkCollision)
        switch (this.prefs.contactType) {
            case ContactType.BOTH_ROBOTS_INSIDE:
                checkIn(robotsInZone, robotsInZone)
                break

            case ContactType.ANY_ROBOT_INSIDE:
                checkIn(robotsInZone, robots)
                break

            case ContactType.RED_ROBOT_INSIDE: {
                const redRobotsInside = robotsInZone.filter(([robot, _]) => robot.alliance === "red")
                checkIn(redRobotsInside, robots)
                break
            }

            case ContactType.BLUE_ROBOT_INSIDE: {
                const blueRobotsInside = robotsInZone.filter(([robot, _]) => robot.alliance === "blue")
                checkIn(blueRobotsInside, robots)
                break
            }
        }

        return collisions
    }

    private penalizeEnteringZone(robot: MirabufSceneObject) {
        if (robot.alliance !== this.prefs.alliance)
            World.scoreTracker.robotPenalty(robot, this.prefs.penaltyPoints ?? 0, "Entered Protected Zone", false)

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
        World.scoreTracker.robotPenalty(
            opposingRobot,
            this.prefs?.penaltyPoints ?? 0,
            `Contact penalty in protected zone`
        )
    }

    public override dispose() {
        super.dispose()
        this.disposeOfRobotBoundingMeshes()
    }

    private disposeOfRobotBoundingMeshes() {
        this._robotBounding?.forEach(m => {
            World.sceneRenderer.removeObject(m)
            m.geometry.dispose()
            const materials = Array.isArray(m.material) ? m.material : [m.material]
            materials.forEach(mat => mat.dispose())
        })
    }
}

export default ProtectedZoneSceneObject
