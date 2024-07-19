import { Array_ThreeMatrix4, JoltMat44_ThreeMatrix4, ThreeQuaternion_JoltQuat, ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "./MirabufSceneObject"
import JOLT from "@/util/loading/JoltSyncLoader"
import World from "@/systems/World"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import { OnContactAddedEvent, OnContactRemovedEvent } from "@/systems/physics/ContactEvents"
import SceneObject from "@/systems/scene/SceneObject"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import EjectableSceneObject from "./EjectableSceneObject"


class ScoringZoneSceneObject extends SceneObject {
    //Official FIRST hex
    static redMaterial =  new THREE.MeshPhongMaterial({
        color: 0xED1C24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    static blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066B3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })  //0x0000ff
    static transparentMaterial = new THREE.MeshPhongMaterial({
        color: 0x0000,
        shininess: 0.0,
        opacity: 0.0,
        transparent: true
    })

    private _parentAssembly: MirabufSceneObject
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation?: THREE.Matrix4

    private _toRender: boolean
    private _prefs?: ScoringZonePreferences
    private _joltBodyId?: Jolt.BodyID
    private _mesh?: THREE.Mesh
    private _collision?: (event: OnContactAddedEvent) => void
    private _collisionRemoved?: (event: OnContactRemovedEvent) => void

    public constructor(parentAssembly: MirabufSceneObject, index: number, render?: boolean) {
        super()

        console.debug("Trying to create scoring zone...")

        this._parentAssembly = parentAssembly
        this._prefs = this._parentAssembly.fieldPreferences?.scoringZones[index]
        this._toRender = render ?? PreferencesSystem.getGlobalPreference<boolean>("RenderScoringZones")
    }

    public Setup(): void {
        if (this._prefs) {
            this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(this._prefs.parentNode ?? this._parentAssembly.rootNodeId)

            if (this._parentBodyId) {

                this._deltaTransformation = Array_ThreeMatrix4(this._prefs.deltaTransformation)

                this._joltBodyId = World.PhysicsSystem.CreateSensor(
                    new JOLT.BoxShapeSettings(
                        new JOLT.Vec3(1,1,1)
                    )
                )

                if (!this._joltBodyId) {
                    console.log("Failed to create scoring zone. No Jolt Body")
                    return
                }
                
                const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform())
                const gizmoTransformation = this._deltaTransformation.clone().premultiply(fieldTransformation)

                const translation = new THREE.Vector3(0, 0, 0)
                const rotation = new THREE.Quaternion(0, 0, 0, 1)
                const scale = new THREE.Vector3(1, 1, 1)
                gizmoTransformation.decompose(translation, rotation, scale)
        
                this._mesh = World.SceneRenderer.CreateBox(
                    new JOLT.Vec3(1,1,1),
                    ScoringZoneSceneObject.blueMaterial
                )

                if (this._toRender) {
                    this._mesh.position.set(translation.x, translation.y, translation.z)
                    this._mesh.rotation.setFromQuaternion(rotation)
                    this._mesh.scale.set(scale.x, scale.y, scale.z)

                    World.SceneRenderer.scene.add(this._mesh)
                }

                World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltVec3(translation))
                World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

                const shapeSettings = new JOLT.BoxShapeSettings(new JOLT.Vec3(scale.x / 2, scale.y / 2, scale.z / 2))
                const shape = shapeSettings.Create()
                World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate)

                this._collision = (event: OnContactAddedEvent) => {
                    const body1 = event.message.body1
                    const body2 = event.message.body2
        
                    if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body2)
                    } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                        this.ZoneCollision(body1)
                    }
                }

                this._collisionRemoved = (event: OnContactRemovedEvent) => {
                    if (this._prefs?.persistentPoints) {
                        const body1 = event.message.GetBody1ID()
                        const body2 = event.message.GetBody2ID()

                        if (body1.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                            this.ZoneCollisionRemoved(body2)
                        } else if (body2.GetIndexAndSequenceNumber() == this._joltBodyId?.GetIndexAndSequenceNumber()) {
                            this.ZoneCollisionRemoved(body1)
                        }
                    }
                }
        
                OnContactAddedEvent.AddListener(this._collision)
                OnContactRemovedEvent.AddListener(this._collisionRemoved)

                console.debug("Scoring zone created successfully")
            }
        }
    }

    public Update(): void {
        if (this._parentBodyId && this._deltaTransformation && this._joltBodyId && this._mesh && this._prefs) {
            this._toRender = PreferencesSystem.getGlobalPreference<boolean>("RenderScoringZones")
            const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(this._parentBodyId).GetWorldTransform())
            const gizmoTransformation = this._deltaTransformation.clone().premultiply(fieldTransformation)

            const translation = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            const scale = new THREE.Vector3(1, 1, 1)
            gizmoTransformation.decompose(translation, rotation, scale)
            // console.log(`update trans: ${translation.toArray()} ${rotation.toArray()} ${scale.toArray()}`)

            if (this._toRender) {
                this._mesh.position.set(translation.x, translation.y, translation.z)
                this._mesh.rotation.setFromQuaternion(rotation)
                this._mesh.scale.set(scale.x, scale.y, scale.z)
                this._mesh.material = this._prefs.alliance == "red" ? ScoringZoneSceneObject.redMaterial : ScoringZoneSceneObject.blueMaterial
            } else {
                this._mesh.material = ScoringZoneSceneObject.transparentMaterial
            }
        
            World.PhysicsSystem.SetBodyPosition(this._joltBodyId, ThreeVector3_JoltVec3(translation))
            World.PhysicsSystem.SetBodyRotation(this._joltBodyId, ThreeQuaternion_JoltQuat(rotation))

            const shapeSettings = new JOLT.BoxShapeSettings(new JOLT.Vec3(scale.x / 2, scale.y / 2, scale.z / 2))
            const shape = shapeSettings.Create()
            World.PhysicsSystem.SetShape(this._joltBodyId, shape.Get(), false, Jolt.EActivation_Activate);


        } else {
            console.debug("Failed to update scoring zone")
        }
    }

    public Dispose(): void {
        console.debug("Destroying scoring zone")

        if (this._joltBodyId) {
            World.PhysicsSystem.DestroyBodyIds(this._joltBodyId)

            if (this._mesh) {
                this._mesh.geometry.dispose()
                ;(this._mesh.material as THREE.Material).dispose()
                World.SceneRenderer.scene.remove(this._mesh)
            }
        }
        
        if (this._collision) OnContactAddedEvent.RemoveListener(this._collision)
        if (this._collisionRemoved) OnContactRemovedEvent.RemoveListener(this._collisionRemoved)
    }

    private ZoneCollision(gpID: Jolt.BodyID) {
        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece && this._prefs) {
            if (this._prefs.alliance == "red") {
                SimulationSystem.redScore += this._prefs.points
            } else {
                SimulationSystem.blueScore += this._prefs.points
            }
            console.log(`Red: ${SimulationSystem.redScore} Blue: ${SimulationSystem.blueScore}`)
        }
    }
    
    // TODO: Add handling for when the ejectable carries the gamepiece out
    private ZoneCollisionRemoved(gpID: Jolt.BodyID) {
        console.log(`Scoring zone ${gpID.GetIndex()} removed from ${this._joltBodyId?.GetIndex()}`)

        const associate = <RigidNodeAssociate>World.PhysicsSystem.GetBodyAssociation(gpID)
        if (associate?.isGamePiece) {
            this.RemoveScore()
        } else {
            const ejectables = [...World.SceneRenderer.sceneObjects.entries()]
                .filter(x => {
                    const y = x[1] instanceof EjectableSceneObject
                    return y
                })
                .map(x => x[1]) as EjectableSceneObject[]
            console.log(`eject ${ejectables.length}`)

            ejectables.forEach(x => {
                if (x.parentBodyId == gpID) {
                    this.RemoveScore()
                }
            })
        }
    }

    private RemoveScore() {
        if (this._prefs)
            if (this._prefs.alliance == "red") {
                SimulationSystem.redScore -= this._prefs.points
                if (SimulationSystem.redScore < 1) SimulationSystem.redScore = 0
            } else {
                SimulationSystem.blueScore -= this._prefs.points
                if (SimulationSystem.blueScore < 1) SimulationSystem.blueScore = 0
            }
        console.log(`Red: ${SimulationSystem.redScore} Blue: ${SimulationSystem.blueScore}`)
    }
}

export default ScoringZoneSceneObject