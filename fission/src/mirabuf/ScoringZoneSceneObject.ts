import type Jolt from "@synthesis.adsk/jolt-physics"
import type * as THREE from "three"
import * as Three from "three"
import { LAYER_GENERAL_DYNAMIC } from "@/systems/physics/PhysicsSystem"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { findListDifference } from "@/util/Utility"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"
import ZoneSceneObject from "./ZoneSceneObject"

class ScoringZoneSceneObject extends ZoneSceneObject<ScoringZonePreferences> {
    public static readonly RED_MATERIAL = new Three.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    public static readonly BLUE_MATERIAL = new Three.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })

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

        const pieces = World.sceneRenderer.mirabufSceneObjects.getPieces()
        const gps = pieces.flatMap(piece =>
            [...piece.mirabufInstance.parser.rigidNodes.values()]
                .map(rn => piece.mechanism.nodeToBody.get(rn.id))
                .filter((id): id is Jolt.BodyID => id !== undefined)
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
    public zoneCollision(gpID: Jolt.BodyID) {
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
        const inGPLayer = World.physicsSystem.getBody(gpID)?.GetObjectLayer() === LAYER_GENERAL_DYNAMIC
        if (!(associate?.isGamePiece || inGPLayer) || !this.prefs) return

        const robotAlliancePoints =
            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -this.prefs.points : this.prefs.points

        if (associate.robotLastInContactWith)
            World.scoreTracker.addPerRobotScore(associate.robotLastInContactWith, scoringFactor * robotAlliancePoints)

        World.scoreTracker.addPoints(this.prefs.alliance, scoringFactor * this.prefs.points)
    }
}

export default ScoringZoneSceneObject
