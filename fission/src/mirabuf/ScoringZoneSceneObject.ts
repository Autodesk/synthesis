import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertArrayToThreeMatrix4, convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import { deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"
import { findListDifference } from "@/util/Utility"
import type MirabufSceneObject from "./MirabufSceneObject"
import type { RigidNodeAssociate } from "./MirabufSceneObject"

class ScoringZoneSceneObject extends SceneObject {
    //Official FIRST hex
    static redMaterial = new THREE.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    static blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    }) //0x0000ff
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
    private _prefs?: ScoringZonePreferences
    private _joltBodyId?: Jolt.BodyID
    private _mesh?: THREE.Mesh
    private _unsubscribers: (() => void)[] = []

    private _gpContacted: Jolt.BodyID[] = []
    private _prevGP: Jolt.BodyID[] = []

    // Scratch WASM objects reused each frame (avoid per-frame heap allocation)
    private _scratchRVec3: Jolt.RVec3
    private _scratchQuat: Jolt.Quat

    // Cache of last written parent transform (7 numbers: tx,ty,tz,rx,ry,rz,rw)
    private _lastTx = NaN
    private _lastTy = NaN
    private _lastTz = NaN
    private _lastRx = NaN
    private _lastRy = NaN
    private _lastRz = NaN
    private _lastRw = NaN

    public get gpContacted() {
        return this._gpContacted
    }

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        super()

        this._parentAssembly = parentAssembly
        this._prefs = this._parentAssembly.fieldPreferences?.scoringZones[index]
        this._toRender = render ?? PreferencesSystem.getGlobalPreference("RenderScoringZones")

        this._scratchRVec3 = new JOLT.RVec3(0, 0, 0)
        this._scratchQuat = new JOLT.Quat(0, 0, 0, 1)
    }

    public setup(): void {
        if (this._prefs) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
                this._prefs.parentNode ?? this._parentAssembly.rootNodeId
            )

            if (this._parentBodyId) {
                // Create a default sensor
                const initVec = new JOLT.Vec3(1, 1, 1)
                const initSettings = new JOLT.BoxShapeSettings(initVec)
                this._joltBodyId = World.physicsSystem.createSensor(initSettings)
                JOLT.destroy(initSettings)
                JOLT.destroy(initVec)
                if (!this._joltBodyId) {
                    console.log("Failed to create scoring zone. No Jolt Body")
                    return
                }

                // Position/rotate/scale sensor to settings
                this._deltaTransformation = convertArrayToThreeMatrix4(this._prefs.deltaTransformation)
                const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(this._parentBodyId).GetWorldTransform()
                )
                const props = deltaFieldTransformsPhysicalProp(this._deltaTransformation, fieldTransformation)

                this._scratchRVec3.Set(props.translation.x, props.translation.y, props.translation.z)
                this._scratchQuat.Set(props.rotation.x, props.rotation.y, props.rotation.z, props.rotation.w)
                World.physicsSystem.setBodyPosition(this._joltBodyId, this._scratchRVec3)
                World.physicsSystem.setBodyRotation(this._joltBodyId, this._scratchQuat)
                const shapeVec = new JOLT.Vec3(props.scale.x / 2, props.scale.y / 2, props.scale.z / 2)
                const shapeSettings = new JOLT.BoxShapeSettings(shapeVec)
                const shape = shapeSettings.Create()
                JOLT.destroy(shapeSettings)
                JOLT.destroy(shapeVec)
                World.physicsSystem.setShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

                // Mesh for the user to visualize sensor
                this._mesh = World.sceneRenderer.createBox(
                    new JOLT.Vec3(1, 1, 1),
                    ScoringZoneSceneObject.transparentMaterial
                )
                World.sceneRenderer.scene.add(this._mesh)

                if (this._toRender) {
                    this._mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
                    this._mesh.rotation.setFromQuaternion(props.rotation)
                    this._mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)
                }

                // Detect new gamepiece listener
                this._unsubscribers.push(
                    EventSystem.listen("OnContactAddedEvent", ({ body1, body2 }) => {
                        if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                            this.zoneCollision(body2)
                        } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                            this.zoneCollision(body1)
                        }
                    })
                )

                // If persistent, detect gamepiece removed listener
                if (this._prefs.persistentPoints) {
                    this._unsubscribers.push(
                        EventSystem.listen("OnContactRemovedEvent", ({ message }) => {
                            if (this._prefs?.persistentPoints) {
                                const body1 = message.GetBody1ID()
                                const body2 = message.GetBody2ID()

                                if (
                                    body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()
                                ) {
                                    this.zoneCollisionRemoved(body2)
                                } else if (
                                    body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()
                                ) {
                                    this.zoneCollisionRemoved(body1)
                                }
                            }
                        })
                    )
                }
            }
        }
    }

    public update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._joltBodyId && this._prefs) {
            // Update translation and rotation only when the parent transform has changed
            const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(this._parentBodyId).GetWorldTransform()
            )
            const props = deltaFieldTransformsPhysicalProp(this._deltaTransformation, fieldTransformation)

            const tx = props.translation.x,
                ty = props.translation.y,
                tz = props.translation.z
            const rx = props.rotation.x,
                ry = props.rotation.y,
                rz = props.rotation.z,
                rw = props.rotation.w

            const transformChanged =
                tx !== this._lastTx ||
                ty !== this._lastTy ||
                tz !== this._lastTz ||
                rx !== this._lastRx ||
                ry !== this._lastRy ||
                rz !== this._lastRz ||
                rw !== this._lastRw

            const toRender = PreferencesSystem.getGlobalPreference("RenderScoringZones")
            const renderChanged = toRender !== this._toRender
            this._toRender = toRender

            if (transformChanged) {
                this._lastTx = tx
                this._lastTy = ty
                this._lastTz = tz
                this._lastRx = rx
                this._lastRy = ry
                this._lastRz = rz
                this._lastRw = rw

                this._scratchRVec3.Set(tx, ty, tz)
                this._scratchQuat.Set(rx, ry, rz, rw)
                World.physicsSystem.setBodyPosition(this._joltBodyId, this._scratchRVec3)
                World.physicsSystem.setBodyRotation(this._joltBodyId, this._scratchQuat)
            }

            // Mesh for visualization
            if (transformChanged || renderChanged) {
                if (this._mesh)
                    if (this._toRender) {
                        this._mesh.position.set(tx, ty, tz)
                        this._mesh.rotation.setFromQuaternion(props.rotation)
                        this._mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)
                        this._mesh.material =
                            this._prefs.alliance == "red"
                                ? ScoringZoneSceneObject.redMaterial
                                : ScoringZoneSceneObject.blueMaterial
                    } else {
                        this._mesh.material = ScoringZoneSceneObject.transparentMaterial
                    }
            }

            // If persistent points, update points based on how many gamepieces in zone
            if (this._prefs.persistentPoints)
                if (this._gpContacted.length != this._prevGP.length) {
                    const { added: gpAdded, removed: gpRemoved } = findListDifference(this._prevGP, this._gpContacted)
                    const points = this._prefs.points

                    ScoreTracker.addPoints(this._prefs.alliance, (gpAdded.length - gpRemoved.length) * points)

                    // Per robot score calculations
                    gpAdded.forEach(gpID => {
                        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
                        const robotAlliancePoints =
                            associate.robotLastInContactWith?.alliance !== this._prefs?.alliance ? -points : points
                        associate.robotLastInContactWith &&
                            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, robotAlliancePoints)
                    })
                    gpRemoved.forEach(gpID => {
                        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
                        const robotAlliancePoints =
                            associate.robotLastInContactWith?.alliance !== this._prefs?.alliance ? -points : points
                        associate.robotLastInContactWith &&
                            ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, -robotAlliancePoints)
                    })

                    this._prevGP = Object.assign([], this._gpContacted)
                }
        } else {
            console.debug("Failed to update scoring zone")
        }
    }

    public reset() {
        this._prevGP = []
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

        JOLT.destroy(this._scratchRVec3)
        JOLT.destroy(this._scratchQuat)

        this._unsubscribers.forEach(unsubscribe => unsubscribe())
    }

    private zoneCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
        if (associate?.isGamePiece && this._prefs) {
            // If persistent, Update() will handle points
            if (this._prefs.persistentPoints) {
                this._gpContacted.push(gpID)
            } else {
                ScoreTracker.addPoints(this._prefs.alliance, this._prefs.points)
                const robotAlliancePoints =
                    associate.robotLastInContactWith?.alliance !== this._prefs?.alliance
                        ? -this._prefs.points
                        : this._prefs.points
                associate.robotLastInContactWith &&
                    ScoreTracker.addPerRobotScore(associate.robotLastInContactWith, robotAlliancePoints)
            }
        }
    }

    // Private gamepiece removal called anytime collision removed from zone. Score update in Update()
    private zoneCollisionRemoved(gpID: Jolt.BodyID) {
        if (this._prefs?.persistentPoints) {
            const associate = <RigidNodeAssociate>World.physicsSystem.getBodyAssociation(gpID)
            if (associate?.isGamePiece) {
                const temp = this._gpContacted.filter(x => {
                    return x.GetIndexAndSequenceNumber() != gpID.GetIndexAndSequenceNumber()
                })
                if (this._gpContacted != temp) this._gpContacted = Object.assign([], temp)
            }
        }
    }

    // Public gamepiece removal called anytime EjectableSceneObject created in case gamepiece was in persistent zone
    // Score update in Update()
    public static removeGamepiece(zone: ScoringZoneSceneObject, gpID: Jolt.BodyID) {
        if (zone._prefs && zone._prefs.persistentPoints) {
            const temp = zone._gpContacted.filter(x => {
                return x.GetIndexAndSequenceNumber() != gpID.GetIndexAndSequenceNumber()
            })
            if (zone._gpContacted != temp) zone._gpContacted = Object.assign([], temp)
        }
    }
}

export default ScoringZoneSceneObject
