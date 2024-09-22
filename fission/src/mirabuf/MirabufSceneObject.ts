import { mirabuf } from "@/proto/mirabuf"
import SceneObject from "../systems/scene/SceneObject"
import MirabufInstance from "./MirabufInstance"
import MirabufParser, { ParseErrorSeverity, RigidNodeId, RigidNodeReadOnly } from "./MirabufParser"
import World from "@/systems/World"
import Jolt from "@barclah/jolt-physics"
import { JoltMat44_ThreeMatrix4 } from "@/util/TypeConversions"
import * as THREE from "three"
import JOLT from "@/util/loading/JoltSyncLoader"
import { BodyAssociate, LayerReserve } from "@/systems/physics/PhysicsSystem"
import Mechanism from "@/systems/physics/Mechanism"
import InputSystem from "@/systems/input/InputSystem"
import { EjectorPreferences, FieldPreferences, IntakePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { MiraType } from "./MirabufLoader"
import IntakeSensorSceneObject from "./IntakeSensorSceneObject"
import EjectableSceneObject from "./EjectableSceneObject"
import Brain from "@/systems/simulation/Brain"
import ScoringZoneSceneObject from "./ScoringZoneSceneObject"
import { SceneOverlayTag } from "@/ui/components/SceneOverlayEvents"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"

const DEBUG_BODIES = false

interface RnDebugMeshes {
    colliderMesh: THREE.Mesh
    comMesh: THREE.Mesh
}

/**
 * The goal with the spotlight assembly is to provide a contextual target assembly
 * the user would like to modifiy. Generally this will be which even assembly was
 * last spawned in, however, systems (such as the configuration UI) can elect
 * assemblies to be in the spotlight when moving from interface to interface.
 */
let spotlightAssembly: number | undefined

export function setSpotlightAssembly(assembly: MirabufSceneObject) {
    spotlightAssembly = assembly.id
}

// TODO: If nothing is in the spotlight, select last entry before defaulting to undefined
export function getSpotlightAssembly(): MirabufSceneObject | undefined {
    return World.SceneRenderer.sceneObjects.get(spotlightAssembly ?? 0) as MirabufSceneObject
}

class MirabufSceneObject extends SceneObject {
    private _assemblyName: string
    private _mirabufInstance: MirabufInstance
    private _mechanism: Mechanism
    private _brain: Brain | undefined

    private _debugBodies: Map<string, RnDebugMeshes> | null
    private _physicsLayerReserve: LayerReserve | undefined

    private _intakePreferences: IntakePreferences | undefined
    private _ejectorPreferences: EjectorPreferences | undefined

    private _fieldPreferences: FieldPreferences | undefined

    private _intakeSensor?: IntakeSensorSceneObject
    private _ejectable?: EjectableSceneObject
    private _scoringZones: ScoringZoneSceneObject[] = []

    private _nameTag: SceneOverlayTag | undefined

    get mirabufInstance() {
        return this._mirabufInstance
    }

    get mechanism() {
        return this._mechanism
    }

    get assemblyName() {
        return this._assemblyName
    }

    get intakePreferences() {
        return this._intakePreferences
    }

    get ejectorPreferences() {
        return this._ejectorPreferences
    }

    get fieldPreferences() {
        return this._fieldPreferences
    }

    public get activeEjectable(): Jolt.BodyID | undefined {
        return this._ejectable?.gamePieceBodyId
    }

    public get miraType(): MiraType {
        return this._mirabufInstance.parser.assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD
    }

    public get rootNodeId(): string {
        return this._mirabufInstance.parser.rootNode
    }

    public get brain() {
        return this._brain
    }

    public constructor(mirabufInstance: MirabufInstance, assemblyName: string, progressHandle?: ProgressHandle) {
        super()

        this._mirabufInstance = mirabufInstance
        this._assemblyName = assemblyName

        progressHandle?.Update("Creating mechanism...", 0.9)

        this._mechanism = World.PhysicsSystem.CreateMechanismFromParser(this._mirabufInstance.parser)
        if (this._mechanism.layerReserve) {
            this._physicsLayerReserve = this._mechanism.layerReserve
        }

        this._debugBodies = null

        this.getPreferences()

        // creating nametag for robots
        if (this.miraType === MiraType.ROBOT) {
            this._nameTag = new SceneOverlayTag(() =>
                this._brain instanceof SynthesisBrain ? this._brain.inputSchemeName : "Not Configured"
            )
        }
    }

    public Setup(): void {
        // Rendering
        this._mirabufInstance.AddToScene(World.SceneRenderer.scene)

        if (DEBUG_BODIES) {
            this._debugBodies = new Map()
            this._mechanism.nodeToBody.forEach((bodyId, rnName) => {
                const body = World.PhysicsSystem.GetBody(bodyId)

                const colliderMesh = this.CreateMeshForShape(body.GetShape())
                const comMesh = World.SceneRenderer.CreateSphere(0.05)
                World.SceneRenderer.scene.add(colliderMesh)
                World.SceneRenderer.scene.add(comMesh)
                ;(comMesh.material as THREE.Material).depthTest = false
                this._debugBodies!.set(rnName, {
                    colliderMesh: colliderMesh,
                    comMesh: comMesh,
                })
            })
        }

        const rigidNodes = this._mirabufInstance.parser.rigidNodes
        this._mechanism.nodeToBody.forEach((bodyId, rigidNodeId) => {
            const rigidNode = rigidNodes.get(rigidNodeId)
            if (!rigidNode) {
                console.warn("Found a RigidNodeId with no related RigidNode. Skipping for now...")
                return
            }
            World.PhysicsSystem.SetBodyAssociation(new RigidNodeAssociate(this, rigidNode, bodyId))
        })

        // Simulation
        if (this.miraType == MiraType.ROBOT) {
            World.SimulationSystem.RegisterMechanism(this._mechanism)
            const simLayer = World.SimulationSystem.GetSimulationLayer(this._mechanism)!
            this._brain = new SynthesisBrain(this._mechanism, this._assemblyName)
            simLayer.SetBrain(this._brain)    
        }

        // Intake
        this.UpdateIntakeSensor()
        this.UpdateScoringZones()

        setSpotlightAssembly(this)
    }

    public Update(): void {
        const brainIndex = this._brain instanceof SynthesisBrain ? this._brain.brainIndex ?? -1 : -1
        if (InputSystem.getInput("eject", brainIndex)) {
            this.Eject()
        }

        /** Updating the position of all mirabuf nodes */
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            if (!this._mirabufInstance.meshes.size) return // if this.dispose() has been ran then return
            const body = World.PhysicsSystem.GetBody(this._mechanism.GetBodyByNodeId(rn.id)!)
            const transform = JoltMat44_ThreeMatrix4(body.GetWorldTransform())
            this.UpdateNodeParts(rn, transform)

            if (isNaN(body.GetPosition().GetX())) {
                const vel = body.GetLinearVelocity()
                const pos = body.GetPosition()
                console.warn(
                    `Invalid Position.\nPosition => ${pos.GetX()}, ${pos.GetY()}, ${pos.GetZ()}\nVelocity => ${vel.GetX()}, ${vel.GetY()}, ${vel.GetZ()}`
                )
            }
            // console.debug(`POSITION: ${body.GetPosition().GetX()}, ${body.GetPosition().GetY()}, ${body.GetPosition().GetZ()}`)

            if (this._debugBodies) {
                const { colliderMesh, comMesh } = this._debugBodies.get(rn.id)!
                colliderMesh.position.setFromMatrixPosition(transform)
                colliderMesh.rotation.setFromRotationMatrix(transform)

                const comTransform = JoltMat44_ThreeMatrix4(body.GetCenterOfMassTransform())

                comMesh.position.setFromMatrixPosition(comTransform)
                comMesh.rotation.setFromRotationMatrix(comTransform)
            }
        })

        this.UpdateBatches()
        this.UpdateNameTag()
    }

    public Dispose(): void {
        if (this._intakeSensor) {
            World.SceneRenderer.RemoveSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        if (this._ejectable) {
            World.SceneRenderer.RemoveSceneObject(this._ejectable.id)
            this._ejectable = undefined
        }

        this._scoringZones.forEach(zone => World.SceneRenderer.RemoveSceneObject(zone.id))
        this._scoringZones = []

        this._mechanism.nodeToBody.forEach(bodyId => {
            World.PhysicsSystem.RemoveBodyAssociation(bodyId)
        })

        this._nameTag?.Dispose()
        World.SimulationSystem.UnregisterMechanism(this._mechanism)
        World.PhysicsSystem.DestroyMechanism(this._mechanism)
        this._mirabufInstance.Dispose(World.SceneRenderer.scene)
        this._debugBodies?.forEach(x => {
            World.SceneRenderer.scene.remove(x.colliderMesh, x.comMesh)
            x.colliderMesh.geometry.dispose()
            x.comMesh.geometry.dispose()
            ;(x.colliderMesh.material as THREE.Material).dispose()
            ;(x.comMesh.material as THREE.Material).dispose()
        })
        this._debugBodies?.clear()
        this._physicsLayerReserve?.Release()

        if (this._brain && this._brain instanceof SynthesisBrain) this._brain?.clearControls()
    }

    public Eject() {
        if (!this._ejectable) {
            return
        }

        this._ejectable.Eject()
        World.SceneRenderer.RemoveSceneObject(this._ejectable.id)
        this._ejectable = undefined
    }

    private CreateMeshForShape(shape: Jolt.Shape): THREE.Mesh {
        const scale = new JOLT.Vec3(1, 1, 1)
        const triangleContext = new JOLT.ShapeGetTriangles(
            shape,
            JOLT.AABox.prototype.sBiggest(),
            shape.GetCenterOfMass(),
            JOLT.Quat.prototype.sIdentity(),
            scale
        )
        JOLT.destroy(scale)

        const vertices = new Float32Array(
            JOLT.HEAP32.buffer,
            triangleContext.GetVerticesData(),
            triangleContext.GetVerticesSize() / Float32Array.BYTES_PER_ELEMENT
        )
        const buffer = new THREE.BufferAttribute(vertices, 3).clone()
        JOLT.destroy(triangleContext)

        const geometry = new THREE.BufferGeometry()
        geometry.setAttribute("position", buffer)
        geometry.computeVertexNormals()

        const material = new THREE.MeshStandardMaterial({
            color: 0x33ff33,
            wireframe: true,
        })
        const mesh = new THREE.Mesh(geometry, material)
        mesh.castShadow = true

        return mesh
    }

    public UpdateNodeParts(rn: RigidNodeReadOnly, transform: THREE.Matrix4) {
        rn.parts.forEach(part => {
            const partTransform = this._mirabufInstance.parser.globalTransforms
                .get(part)!
                .clone()
                .premultiply(transform)
            const meshes = this._mirabufInstance.meshes.get(part) ?? []
            meshes.forEach(([batch, id]) => batch.setMatrixAt(id, partTransform))
        })
    }

    /** Updates the batch computations */
    private UpdateBatches() {
        this._mirabufInstance.batches.forEach(x => {
            x.computeBoundingBox()
            x.computeBoundingSphere()
        })
    }

    /** Updates the position of the nametag relative to the robots position */
    private UpdateNameTag() {
        if (this._nameTag && PreferencesSystem.getGlobalPreference<boolean>("RenderSceneTags")) {
            const boundingBox = this.ComputeBoundingBox()
            this._nameTag.position = World.SceneRenderer.WorldToPixelSpace(
                new THREE.Vector3(
                    (boundingBox.max.x + boundingBox.min.x) / 2,
                    boundingBox.max.y + 0.1,
                    (boundingBox.max.z + boundingBox.min.z) / 2
                )
            )
        }
    }

    public UpdateIntakeSensor() {
        if (this._intakeSensor) {
            World.SceneRenderer.RemoveSceneObject(this._intakeSensor.id)
            this._intakeSensor = undefined
        }

        // Do we have an intake, and is it something other than the default. Config will default to root node at least.
        if (this._intakePreferences && this._intakePreferences.parentNode) {
            this._intakeSensor = new IntakeSensorSceneObject(this)
            World.SceneRenderer.RegisterSceneObject(this._intakeSensor)
        }
    }

    public SetEjectable(bodyId?: Jolt.BodyID, removeExisting: boolean = false): boolean {
        if (this._ejectable) {
            if (!removeExisting) {
                return false
            }

            World.SceneRenderer.RemoveSceneObject(this._ejectable.id)
            this._ejectable = undefined
        }

        if (!this._ejectorPreferences || !this._ejectorPreferences.parentNode || !bodyId) {
            console.log(`Configure an ejectable first.`)
            return false
        }

        this._ejectable = new EjectableSceneObject(this, bodyId)
        World.SceneRenderer.RegisterSceneObject(this._ejectable)
        return true
    }

    public UpdateScoringZones(render?: boolean) {
        this._scoringZones.forEach(zone => World.SceneRenderer.RemoveSceneObject(zone.id))
        this._scoringZones = []

        if (this._fieldPreferences && this._fieldPreferences.scoringZones) {
            for (let i = 0; i < this._fieldPreferences.scoringZones.length; i++) {
                const newZone = new ScoringZoneSceneObject(
                    this,
                    i,
                    render ?? PreferencesSystem.getGlobalPreference("RenderScoringZones")
                )
                this._scoringZones.push(newZone)
                World.SceneRenderer.RegisterSceneObject(newZone)
            }
        }
    }

    /**
     * Calculates the bounding box of the mirabuf object.
     *
     * @returns The bounding box of the mirabuf object.
     */
    private ComputeBoundingBox(): THREE.Box3 {
        const box = new THREE.Box3()
        this._mirabufInstance.batches.forEach(batch => {
            if (batch.boundingBox) box.union(batch.boundingBox)
        })

        return box
    }

    /**
     * Once a gizmo is created and attached to this mirabuf object, this will be executed to align the gizmo correctly.
     * 
     * @param gizmo Gizmo attached to the mirabuf object
     */
    public PostGizmoCreation(gizmo: GizmoSceneObject) {
        // TODO: Move to the center of the bot/field
    }

    private getPreferences(): void {
        this._intakePreferences = PreferencesSystem.getRobotPreferences(this.assemblyName)?.intake
        this._ejectorPreferences = PreferencesSystem.getRobotPreferences(this.assemblyName)?.ejector

        this._fieldPreferences = PreferencesSystem.getFieldPreferences(this.assemblyName)
    }

    public EnablePhysics() {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.PhysicsSystem.EnablePhysicsForBody(this._mechanism.GetBodyByNodeId(rn.id)!)
        })
    }

    public DisablePhysics() {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.PhysicsSystem.DisablePhysicsForBody(this._mechanism.GetBodyByNodeId(rn.id)!)
        })
    }

    public GetRootNodeId(): Jolt.BodyID | undefined {
        return this._mechanism.GetBodyByNodeId(this._mechanism.rootBody)
    }
}

export async function CreateMirabuf(
    assembly: mirabuf.Assembly,
    progressHandle?: ProgressHandle
): Promise<MirabufSceneObject | null | undefined> {
    const parser = new MirabufParser(assembly, progressHandle)
    if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
        console.error(`Assembly Parser produced significant errors for '${assembly.info!.name!}'`)
        return
    }

    return new MirabufSceneObject(new MirabufInstance(parser), assembly.info!.name!, progressHandle)
}

/**
 * Body association to a rigid node with a given mirabuf scene object.
 */
export class RigidNodeAssociate extends BodyAssociate {
    public readonly sceneObject: MirabufSceneObject

    public readonly rigidNode: RigidNodeReadOnly
    public get rigidNodeId(): RigidNodeId {
        return this.rigidNode.id
    }

    public get isGamePiece(): boolean {
        return this.rigidNode.isGamePiece
    }

    public constructor(sceneObject: MirabufSceneObject, rigidNode: RigidNodeReadOnly, body: Jolt.BodyID) {
        super(body)
        this.sceneObject = sceneObject
        this.rigidNode = rigidNode
    }
}

export default MirabufSceneObject
