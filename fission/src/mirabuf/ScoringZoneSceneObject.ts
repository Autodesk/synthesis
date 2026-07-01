import type Jolt from "@azaleacolburn/jolt-physics"
import type * as THREE from "three"
import * as Three from "three"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { findListDifference } from "@/util/Utility"
import MirabufSceneObject from "./MirabufSceneObject"
import { RigidNodeAssociate } from "./MirabufSceneObject"
import ZoneSceneObject from "./ZoneSceneObject"
import JOLT from "@/util/loading/JoltSyncLoader"

class ScoringZoneSceneObject extends ZoneSceneObject<ScoringZonePreferences> {
    public static readonly redMaterial = new Three.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    public static readonly blueMaterial = new Three.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })

    private _prevGPs: Jolt.BodyID[] = []

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ScoringZoneSceneObject.redMaterial, blue: ScoringZoneSceneObject.blueMaterial }
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        const prefs = parentAssembly.fieldPreferences?.scoringZones[index]
        if (!prefs) {
            console.error(
                "Tried to create ScoringZoneSceneObject with undefined preferences. This is necessarily a bug in Synthesis and should be hotfixed. If you are a user seeing this, please open an issue on our github: https://github.com/Autodesk/synthesis."
            )
        }

        if ("persistentPoints" in prefs!) {
            prefs.shouldPointsAccumulate = !prefs.persistentPoints
            delete prefs.persistentPoints
        }

        super(parentAssembly, prefs!, "RenderScoringZones")
    }

    public checkObjectsInZone(): void {
        if (!this.bounding) return

        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!field) return

        const gps = [...field.mirabufInstance.parser.rigidNodes.values()]
            .filter(rn => rn.isGamePiece)
            .map(rn => field.mechanism.nodeToBody.get(rn.id)!) as Jolt.BodyID[]

        const gamePiecesContacting = gps.filter(gpID => {
            const gp = World.physicsSystem.getBody(gpID)!
            const gpBounding = gp.GetWorldSpaceBounds()

            const overlaps = this.bounding?.OverlapsAABox(gpBounding)
            JOLT.destroy(gpBounding)

            return overlaps
        })

        const { added, removed } = findListDifference(this._prevGPs, gamePiecesContacting)

        added.forEach(gpID => this.zoneCollision(gpID))
        if (!this.prefs.shouldPointsAccumulate) {
            removed.forEach(gpID => this.zoneCollisionRemovedNoAccumulation(gpID))
        }

        this._prevGPs = gamePiecesContacting
    }

    public override update(): void {
        super.update()
    }

    public reset() {
        this._prevGPs.length = 0
    }

    /// Updates points for alliance and robot when game piece enters this scoring zone
    private zoneCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)

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
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)

        ScoreTracker.addPoints(this.prefs.alliance, -this.prefs.points)
        const robotAlliancePoints =
            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -this.prefs.points : this.prefs.points
        associate.robotLastInContactWith &&
            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, -robotAlliancePoints)
    }
}

export default ScoringZoneSceneObject
