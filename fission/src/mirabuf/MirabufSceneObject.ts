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
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import InputSystem from "@/systems/input/InputSystem"
import TransformGizmos from "@/ui/components/TransformGizmos"
import { EjectorPreferences, IntakePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { MiraType } from "./MirabufLoader"
import IntakeSensorSceneObject from "./IntakeSensorSceneObject"
import EjectableSceneObject from "./EjectableSceneObject"

const DEBUG_BODIES = false

interface RnDebugMeshes {
    colliderMesh: THREE.Mesh
    comMesh: THREE.Mesh
}

class MirabufSceneObject extends SceneObject {
    private _assemblyName: string
    private _mirabufInstance: MirabufInstance
    private _mechanism: Mechanism
    private _brain: SynthesisBrain | undefined

    private _debugBodies: Map<string, RnDebugMeshes> | null
    private _physicsLayerReserve: LayerReserve | undefined

    private _transformGizmos: TransformGizmos | undefined

    private _intakePreferences: IntakePreferences | undefined
    private _ejectorPreferences: EjectorPreferences | undefined

    private _intakeSensor?: IntakeSensorSceneObject
    private _ejectable?: EjectableSceneObject

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

    public get activeEjectable(): Jolt.BodyID | undefined {
        return this._ejectable?.gamePieceBodyId
    }

    public get miraType(): MiraType {
        return this._mirabufInstance.parser.assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD
    }

    public get rootNodeId(): string {
        return this._mirabufInstance.parser.rootNode
    }

    public constructor(mirabufInstance: MirabufInstance, assemblyName: string) {
        super()

        this._mirabufInstance = mirabufInstance
        this._assemblyName = assemblyName

        this._mechanism = World.PhysicsSystem.CreateMechanismFromParser(this._mirabufInstance.parser)
        if (this._mechanism.layerReserve) {
            this._physicsLayerReserve = this._mechanism.layerReserve
        }

        this._debugBodies = null

        this.EnableTransformControls() // adding transform gizmo to mirabuf object on its creation

        this.getPreferences()
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
        World.SimulationSystem.RegisterMechanism(this._mechanism)
        const simLayer = World.SimulationSystem.GetSimulationLayer(this._mechanism)!
        this._brain = new SynthesisBrain(this._mechanism, this._assemblyName)
        simLayer.SetBrain(this._brain)

        // Intake
        this.UpdateIntakeSensor()
    }

    public Update(): void {
        if (InputSystem.currentModifierState.ctrl && InputSystem.currentModifierState.shift && this._ejectable) {
            this.Eject()
        }

        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            if (!this._mirabufInstance.meshes.size) return // if this.dispose() has been ran then return
            const body = World.PhysicsSystem.GetBody(this._mechanism.GetBodyByNodeId(rn.id)!)
            const transform = JoltMat44_ThreeMatrix4(body.GetWorldTransform())
            rn.parts.forEach(part => {
                const partTransform = this._mirabufInstance.parser.globalTransforms
                    .get(part)!
                    .clone()
                    .premultiply(transform)
                const meshes = this._mirabufInstance.meshes.get(part) ?? []
                meshes.forEach(([batch, id]) => batch.setMatrixAt(id, partTransform))
            })

            /**
             * Update the position and rotation of the body to match the position of the transform gizmo.
             *
             * This block of code should only be executed if the transform gizmo exists.
             */
            if (this._transformGizmos) {
                if (InputSystem.isKeyPressed("Enter")) {
                    // confirming placement of the mirabuf object
                    this.DisableTransformControls()
                    return
                } else if (InputSystem.isKeyPressed("Escape")) {
                    // cancelling the creation of the mirabuf scene object
                    World.SceneRenderer.RemoveSceneObject(this.id)
                    return
                }

                // if the gizmo is being dragged, copy the mesh position and rotation to the Mirabuf body
                if (this._transformGizmos.isBeingDragged()) {
                    this._transformGizmos.UpdateMirabufPositioning(this, rn)
                    World.PhysicsSystem.DisablePhysicsForBody(this._mechanism.GetBodyByNodeId(rn.id)!)
                }
            }

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

        this._mirabufInstance.batches.forEach(x => {
            x.computeBoundingBox()
            x.computeBoundingSphere()
        })
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

        this._mechanism.nodeToBody.forEach(bodyId => {
            World.PhysicsSystem.RemoveBodyAssocation(bodyId)
        })

        this.DisableTransformControls()
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

        this._brain?.clearControls()
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
            return false
        }

        this._ejectable = new EjectableSceneObject(this, bodyId)
        World.SceneRenderer.RegisterSceneObject(this._ejectable)
        return true
    }

    /**
     * Changes the mode of the mirabuf object from being interacted with to being placed.
     */
    public EnableTransformControls(): void {
        if (this._transformGizmos) return

        this._transformGizmos = new TransformGizmos(
            new THREE.Mesh(
                new THREE.SphereGeometry(3.0),
                new THREE.MeshBasicMaterial({ color: 0x000000, transparent: true, opacity: 0 })
            )
        )
        this._transformGizmos.AddMeshToScene()
        this._transformGizmos.CreateGizmo("translate", 5.0)

        this.DisablePhysics()
    }

    /**
     * Changes the mode of the mirabuf object from being placed to being interacted with.
     */
    public DisableTransformControls(): void {
        if (!this._transformGizmos) return
        this._transformGizmos?.RemoveGizmos()
        this._transformGizmos = undefined
        this.EnablePhysics()
    }

    private getPreferences(): void {
        this._intakePreferences = PreferencesSystem.getRobotPreferences(this.assemblyName)?.intake
        this._ejectorPreferences = PreferencesSystem.getRobotPreferences(this.assemblyName)?.ejector
    }

    private EnablePhysics() {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.PhysicsSystem.EnablePhysicsForBody(this._mechanism.GetBodyByNodeId(rn.id)!)
        })
    }

    private DisablePhysics() {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            World.PhysicsSystem.DisablePhysicsForBody(this._mechanism.GetBodyByNodeId(rn.id)!)
        })
    }

    public GetRootNodeId(): Jolt.BodyID | undefined {
        return this._mechanism.GetBodyByNodeId(this._mechanism.rootBody)
    }
}

export async function CreateMirabuf(assembly: mirabuf.Assembly): Promise<MirabufSceneObject | null | undefined> {
    const parser = new MirabufParser(assembly)
    if (parser.maxErrorSeverity >= ParseErrorSeverity.Unimportable) {
        console.error(`Assembly Parser produced significant errors for '${assembly.info!.name!}'`)
        return
    }

    return new MirabufSceneObject(new MirabufInstance(parser), assembly.info!.name!)
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
