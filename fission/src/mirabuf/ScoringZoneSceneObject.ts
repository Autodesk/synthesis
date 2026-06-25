import type Jolt from "@azaleacolburn/jolt-physics"
import type * as THREE from "three"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { findListDifference } from "@/util/Utility"
import MirabufSceneObject from "./MirabufSceneObject"
import { RigidNodeAssociate } from "./MirabufSceneObject"
import ZoneSceneObject from "./ZoneSceneObject"
import assert from "assert"
import JOLT from "@/util/loading/JoltSyncLoader"

class ScoringZoneSceneObject extends ZoneSceneObject<ScoringZonePreferences> {
    private _prevGPs: Jolt.BodyID[] = []

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ZoneSceneObject.lightRedMaterial, blue: ZoneSceneObject.lightBlueMaterial }
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        const prefs = parentAssembly.fieldPreferences?.scoringZones[index]
        if (!prefs) return

        if ("persistentPoints" in prefs) {
            prefs.shouldPointsAccumulate = !prefs.persistentPoints
            delete prefs.persistentPoints
        }

        super(parentAssembly, prefs, "RenderScoringZones")
    }

    public checkObjectsInZone(): void {
        if (!this.bounding) return

        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!field) return

        const gamePiecesContacting = field.activeEjectables.filter(gpID => {
            const gp = World.physicsSystem.getBody(gpID)!
            const objBounding = gp.GetWorldSpaceBounds()

            return this.bounding?.OverlapsAABox(objBounding)
        })

        gamePiecesContacting.forEach(this.zoneCollision)

        const { added: _, removed } = findListDifference(this._prevGPs, gamePiecesContacting)
        if (!this.prefs.shouldPointsAccumulate) {
            removed.forEach(this.zoneCollisionRemovedNoAccumulation)
        }

        this._prevGPs = gamePiecesContacting
    }

    public override update(): void {
        super.update()
    }

    public reset() {
        this._prevGPs.length = 0
    }

    public dispose(): void {
        JOLT.destroy(this.bounding)
    }

    /// Updates points for alliance and robot when game piece enters this scoring zone
    private zoneCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        if (!associate?.isGamePiece || !this.prefs) return

        ScoreTracker.addPoints(this.prefs.alliance, this.prefs.points)
        const robotAlliancePoints =
            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -this.prefs.points : this.prefs.points
        associate.robotLastInContactWith &&
            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, robotAlliancePoints)
    }

    /**
     * Updates points for alliance and robot when game piece is removed from this scoring zone and points should not accumulate
     * Basically removes points from the alliance and robot
     */
    private zoneCollisionRemovedNoAccumulation(gpID: Jolt.BodyID) {
        // TODO remove asserts
        assert(!this.prefs?.shouldPointsAccumulate)

        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        assert(associate.isGamePiece)

        ScoreTracker.addPoints(this.prefs.alliance, -this.prefs.points)
        const robotAlliancePoints =
            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -this.prefs.points : this.prefs.points
        associate.robotLastInContactWith &&
            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, -robotAlliancePoints)
    }

    // TODO
    // Figure out what needs to be done here
    // Public gamepiece removal called anytime `EjectableSceneObject` created in case gamepiece was in persistent zone
    // Score update in Update()
    // public static removeGamepiece(zone: ScoringZoneSceneObject, gpID: Jolt.BodyID) {
    //     if (zone.prefs && !zone.prefs.shouldPointsAccumulate) {
    //         const temp = zone._gpContacted.filter(x => {
    //             return x.GetIndexAndSequenceNumber() != gpID.GetIndexAndSequenceNumber()
    //         })
    //         if (zone._gpContacted != temp) zone._gpContacted = Object.assign([], temp)
    //     }
    // }
}

export default ScoringZoneSceneObject
