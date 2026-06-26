import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { findListDifference } from "@/util/Utility"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"
import ZoneSceneObject from "./ZoneSceneObject"

class ScoringZoneSceneObject extends ZoneSceneObject<ScoringZonePreferences> {
    private _gpContacted: Jolt.BodyID[] = []
    private _prevGP: Jolt.BodyID[] = []

    public get materials(): { red: THREE.MeshPhongMaterial; blue: THREE.MeshPhongMaterial } {
        return { red: ZoneSceneObject.lightRedMaterial, blue: ZoneSceneObject.lightBlueMaterial }
    }

    public get gpContacted() {
        return this._gpContacted
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        const prefs = parentAssembly.fieldPreferences?.scoringZones[index]
        if (prefs && "persistentPoints" in prefs) {
            prefs.shouldPointsAccumulate = !prefs.persistentPoints
            delete prefs.persistentPoints

            // NOTE
            // I'm pretty sure it's passed by reference, but just in case
            parentAssembly.fieldPreferences.scoringZones[index] = prefs
        }

        super(parentAssembly, parentAssembly.fieldPreferences?.scoringZones[index]!, "RenderScoringZones", render)

        this.toRender = PreferencesSystem.getGlobalPreference("RenderScoringZones")
    }

    public setupCollisionSubscribers() {
        // Detect new gamepiece listener
        this.unsubscribers.push(
            EventSystem.listen("OnContactAddedEvent", ({ body1, body2 }) => {
                if (body1.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                    this.zoneCollision(body2)
                } else if (body2.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                    this.zoneCollision(body1)
                }
            })
        )

        // If persistent, detect gamepiece removed listener
        this.unsubscribers.push(
            EventSystem.listen("OnContactRemovedEvent", ({ message }) => {
                if (!this.prefs?.shouldPointsAccumulate) {
                    const body1 = message.GetBody1ID()
                    const body2 = message.GetBody2ID()

                    if (body1.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollisionRemoved(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this.joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.zoneCollisionRemoved(body1)
                    }
                }
            })
        )
    }

    public override update(): void {
        if (this.parentBodyId && this.deltaTransformation && this.joltBodyId && this.prefs) {
            super.update()

            // If persistent points, update points based on how many gamepieces in zone
            if (!this.prefs.shouldPointsAccumulate) {
                if (this._gpContacted.length != this._prevGP.length) {
                    const { added: gpAdded, removed: gpRemoved } = findListDifference(this._prevGP, this._gpContacted)
                    const points = this.prefs.points

                    ScoreTracker.addPoints(this.prefs.alliance, (gpAdded.length - gpRemoved.length) * points)

                    // Per robot score calculations
                    gpAdded.forEach(gpID => {
                        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
                        const robotAlliancePoints =
                            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -points : points
                        associate.robotLastInContactWith &&
                            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, robotAlliancePoints)
                    })
                    gpRemoved.forEach(gpID => {
                        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
                        const robotAlliancePoints =
                            associate.robotLastInContactWith?.alliance !== this.prefs?.alliance ? -points : points
                        associate.robotLastInContactWith &&
                            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, -robotAlliancePoints)
                    })

                    this._prevGP = Object.assign([], this._gpContacted)
                }
            }
        } else {
            console.debug("Failed to update scoring zone")
        }
    }

    public reset() {
        this._prevGP = []
    }

    public dispose(): void {
        if (this.joltBodyId) {
            World.physicsSystem.destroyBodyIds(this.joltBodyId)
            if (this.mesh) {
                this.mesh.geometry.dispose()
                ;(this.mesh.material as THREE.Material).dispose()
                World.sceneRenderer.scene.remove(this.mesh)
            }
        }

        this.unsubscribers.forEach(unsubscribe => unsubscribe())
    }

    private zoneCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        if (associate?.isGamePiece && this.prefs) {
            // If persistent, Update() will handle points
            if (!this.prefs.shouldPointsAccumulate) {
                this._gpContacted.push(gpID)
            } else {
                ScoreTracker.addPoints(this.prefs.alliance, this.prefs.points)
                const robotAlliancePoints =
                    associate.robotLastInContactWith?.alliance !== this.prefs?.alliance
                        ? -this.prefs.points
                        : this.prefs.points
                associate.robotLastInContactWith &&
                    ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, robotAlliancePoints)
            }
        }
    }

    // Private gamepiece removal called anytime collision removed from zone. Score update in Update()
    private zoneCollisionRemoved(gpID: Jolt.BodyID) {
        if (!this.prefs?.shouldPointsAccumulate) {
            const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
            if (associate?.isGamePiece) {
                const temp = this._gpContacted.filter(x => {
                    return x.GetIndexAndSequenceNumber() != gpID.GetIndexAndSequenceNumber()
                })
                if (this._gpContacted != temp) this._gpContacted = Object.assign([], temp)
            }
        }
    }

    // Public gamepiece removal called anytime `EjectableSceneObject` created in case gamepiece was in persistent zone
    // Score update in Update()
    public static removeGamepiece(zone: ScoringZoneSceneObject, gpID: Jolt.BodyID) {
        if (zone.prefs && !zone.prefs.shouldPointsAccumulate) {
            const temp = zone._gpContacted.filter(x => {
                return x.GetIndexAndSequenceNumber() != gpID.GetIndexAndSequenceNumber()
            })
            if (zone._gpContacted != temp) zone._gpContacted = Object.assign([], temp)
        }
    }
}

export default ScoringZoneSceneObject
