import type Jolt from "@synthesis.adsk/jolt-physics"
import type * as THREE from "three"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { findListDifference } from "@/util/Utility"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"
import ZoneSceneObject, { createZoneMaterial } from "./ZoneSceneObject"

class ScoringZoneSceneObject extends ZoneSceneObject<ScoringZonePreferences> {
    public static readonly RED_MATERIAL = createZoneMaterial(0xed1c24, 0.7)
    public static readonly BLUE_MATERIAL = createZoneMaterial(0x0066b3, 0.7)

    private _prevGPs: Jolt.BodyID[] = []

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ScoringZoneSceneObject.RED_MATERIAL, blue: ScoringZoneSceneObject.BLUE_MATERIAL }
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number) {
        const prefs = parentAssembly.fieldPreferences?.scoringZones[index]!
        if ("persistentPoints" in prefs) {
            prefs.shouldPointsAccumulate = !prefs.persistentPoints
            delete prefs.persistentPoints
        }

        super(parentAssembly, prefs, "RenderScoringZones")
    }

    public override checkObjectsInZone(): void {
        if (!this.bounding) return

        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!field) return

        const gamepieces = [...field.mirabufInstance.parser.rigidNodes.values()].filter(rn => rn.isGamePiece)
        const gps = gamepieces
            .map(rn => field.mechanism.nodeToBody.get(rn.id))
            .filter((id): id is Jolt.BodyID => id !== undefined)

        if (gamepieces.length > 0 && gps.length === 0)
            console.warn(
                `ScoringZone: ${gamepieces.length} game piece nodes exist but none have body IDs in nodeToBody`
            )

        const gamePiecesContacting = gps.filter(gpID => {
            const gp = World.physicsSystem.getBody(gpID)
            if (!gp) return false

            const gpBounding = gp.GetWorldSpaceBounds() // STATIC_ALIAS
            const overlaps = this.bounding?.OverlapsAABox(gpBounding)

            return overlaps
        })

        const { added, removed } = findListDifference(this._prevGPs, gamePiecesContacting)

        added.forEach(gpID => this.zoneCollision(gpID))
        if (!this.prefs.shouldPointsAccumulate) removed.forEach(gpID => this.zoneCollisionRemovedNoAccumulation(gpID))

        this._prevGPs = gamePiecesContacting
    }

    public override update(): void {
        super.update()
    }

    public reset() {
        this._prevGPs.length = 0
    }

    /**
     * Updates points for alliance and robot when game piece enters this scoring zone
     */
    private zoneCollision(gpID: Jolt.BodyID) {
        this.zoneCollisionGeneric(gpID, 1)
    }

    /**
     * Updates points for alliance and robot when game piece is removed from this scoring zone and points should not accumulate
     * i.e. Removes points from the alliance and robot
     */
    private zoneCollisionRemovedNoAccumulation(gpID: Jolt.BodyID) {
        this.zoneCollisionGeneric(gpID, -1)
    }

    private zoneCollisionGeneric(gpID: Jolt.BodyID, scoringFactor: number): void {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        const robotAlliancePoints =
            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -this.prefs.points : this.prefs.points

        if (associate.robotLastInContactWith)
            World.scoreTracker.addPerRobotScore(associate.robotLastInContactWith, scoringFactor * robotAlliancePoints)

        World.scoreTracker.addPoints(this.prefs.alliance, scoringFactor * this.prefs.points)
    }
}

export default ScoringZoneSceneObject
