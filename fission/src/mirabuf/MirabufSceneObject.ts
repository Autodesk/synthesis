import { mirabuf } from "@/proto/mirabuf"
import SceneObject from "../systems/scene/SceneObject"
import MirabufInstance from "./MirabufInstance"
import MirabufParser, { ParseErrorSeverity } from "./MirabufParser"
import World from "@/systems/World"
import Jolt from "@barclah/jolt-physics"
import { JoltMat44_ThreeMatrix4 } from "@/util/TypeConversions"
import * as THREE from "three"
import JOLT from "@/util/loading/JoltSyncLoader"
import { LayerReserve } from "@/systems/physics/PhysicsSystem"
import Mechanism from "@/systems/physics/Mechanism"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"

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
    private _physicsLayerReserve: LayerReserve | undefined = undefined

    public constructor(mirabufInstance: MirabufInstance, assemblyName: string) {
        super()

        this._mirabufInstance = mirabufInstance
        this._assemblyName = assemblyName

        this._mechanism = World.PhysicsSystem.CreateMechanismFromParser(this._mirabufInstance.parser)
        if (this._mechanism.layerReserve) {
            this._physicsLayerReserve = this._mechanism.layerReserve
        }

        this._debugBodies = null
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

        // Simulation
        World.SimulationSystem.RegisterMechanism(this._mechanism)
        const simLayer = World.SimulationSystem.GetSimulationLayer(this._mechanism)!
        this._brain = new SynthesisBrain(this._mechanism, this._assemblyName)
        simLayer.SetBrain(this._brain)
    }

    public Update(): void {
        this._mirabufInstance.parser.rigidNodes.forEach(rn => {
            const body = World.PhysicsSystem.GetBody(this._mechanism.GetBodyByNodeId(rn.id)!)
            const transform = JoltMat44_ThreeMatrix4(body.GetWorldTransform())
            rn.parts.forEach(part => {
                const partTransform = this._mirabufInstance.parser.globalTransforms
                    .get(part)!
                    .clone()
                    .premultiply(transform)
                this._mirabufInstance.meshes.get(part)!.forEach(mesh => {
                    mesh.position.setFromMatrixPosition(partTransform)
                    mesh.rotation.setFromRotationMatrix(partTransform)
                })
            })

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
    }

    public Dispose(): void {
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

export default MirabufSceneObject
