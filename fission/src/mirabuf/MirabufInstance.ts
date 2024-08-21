import * as THREE from "three"
import { mirabuf } from "../proto/mirabuf"
import MirabufParser, { ParseErrorSeverity } from "./MirabufParser.ts"
import World from "@/systems/World.ts"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData.ts"

type MirabufPartInstanceGUID = string

const WIREFRAME = false

export enum MaterialStyle {
    Regular = 0,
    Normals = 1,
    Toon = 2,
}

export const matToString = (mat: THREE.Matrix4) => {
    const arr = mat.toArray()
    return (
        `[\n${arr[0].toFixed(4)}, ${arr[4].toFixed(4)}, ${arr[8].toFixed(4)}, ${arr[12].toFixed(4)},\n` +
        `${arr[1].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[13].toFixed(4)},\n` +
        `${arr[2].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[14].toFixed(4)},\n` +
        `${arr[3].toFixed(4)}, ${arr[7].toFixed(4)}, ${arr[11].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
    )
}

export const miraMatToString = (mat: mirabuf.ITransform) => {
    const arr = mat.spatialMatrix!
    return (
        `[\n${arr[0].toFixed(4)}, ${arr[1].toFixed(4)}, ${arr[2].toFixed(4)}, ${arr[3].toFixed(4)},\n` +
        `${arr[4].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[7].toFixed(4)},\n` +
        `${arr[8].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[11].toFixed(4)},\n` +
        `${arr[12].toFixed(4)}, ${arr[13].toFixed(4)}, ${arr[14].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
    )
}

let nextFillerMaterial = 0
const fillerMaterials = [
    new THREE.MeshStandardMaterial({
        color: 0xe32b50,
    }),
    new THREE.MeshStandardMaterial({
        color: 0x4ccf57,
    }),
    new THREE.MeshStandardMaterial({
        color: 0xcf4cca,
    }),
    new THREE.MeshStandardMaterial({
        color: 0x585fed,
    }),
    new THREE.MeshStandardMaterial({
        color: 0xade04f,
    }),
]

const transformVerts = (mesh: mirabuf.IMesh) => {
    const newVerts = new Float32Array(mesh.verts!.length)
    for (let i = 0; i < mesh.verts!.length; i += 3) {
        newVerts[i] = mesh.verts!.at(i)! / 100.0
        newVerts[i + 1] = mesh.verts!.at(i + 1)! / 100.0
        newVerts[i + 2] = mesh.verts!.at(i + 2)! / 100.0
    }
    return newVerts
}

const transformNorms = (mesh: mirabuf.IMesh) => {
    const newNorms = new Float32Array(mesh.normals!.length)
    for (let i = 0; i < mesh.normals!.length; i += 3) {
        const normLength = Math.sqrt(
            mesh.normals!.at(i)! * mesh.normals!.at(i)! +
                mesh.normals!.at(i + 1)! * mesh.normals!.at(i + 1)! +
                mesh.normals!.at(i + 2)! * mesh.normals!.at(i + 2)!
        )

        newNorms[i] = mesh.normals!.at(i)! / normLength
        newNorms[i + 1] = mesh.normals!.at(i + 1)! / normLength
        newNorms[i + 2] = mesh.normals!.at(i + 2)! / normLength
    }
    return newNorms
}

const transformGeometry = (geometry: THREE.BufferGeometry, mesh: mirabuf.IMesh) => {
    const newVerts = transformVerts(mesh)
    const newNorms = transformNorms(mesh)

    geometry.setAttribute("position", new THREE.BufferAttribute(new Float32Array(newVerts), 3))
    geometry.setAttribute("normal", new THREE.BufferAttribute(new Float32Array(newNorms), 3))
    geometry.setAttribute("uv", new THREE.BufferAttribute(new Float32Array(mesh.uv!), 2))
    geometry.setIndex(mesh.indices!)
}

class MirabufInstance {
    private _mirabufParser: MirabufParser
    private _materials: Map<string, THREE.Material>
    private _meshes: Map<MirabufPartInstanceGUID, Array<[THREE.BatchedMesh, number]>>
    private _batches: Array<THREE.BatchedMesh>

    public get parser() {
        return this._mirabufParser
    }
    public get materials() {
        return this._materials
    }
    public get meshes() {
        return this._meshes
    }
    public get batches() {
        return this._batches
    }

    public constructor(parser: MirabufParser, materialStyle?: MaterialStyle, progressHandle?: ProgressHandle) {
        if (parser.errors.some(x => x[0] >= ParseErrorSeverity.Unimportable))
            throw new Error("Parser has significant errors...")

        this._mirabufParser = parser
        this._materials = new Map()
        this._meshes = new Map()
        this._batches = new Array<THREE.BatchedMesh>()

        progressHandle?.Update("Loading materials...", 0.4)
        this.LoadMaterials(materialStyle ?? MaterialStyle.Regular)

        progressHandle?.Update("Creating meshes...", 0.5)
        this.CreateMeshes()
    }

    /**
     * Parses all mirabuf appearances into ThreeJS and Jolt materials.
     */
    private LoadMaterials(materialStyle: MaterialStyle) {
        Object.entries(this._mirabufParser.assembly.data!.materials!.appearances!).forEach(
            ([appearanceId, appearance]) => {
                const { A, B, G, R } = appearance.albedo ?? {}
                const [hex, opacity] =
                    A && B && G && R ? [(A << 24) | (R << 16) | (G << 8) | B, A / 255.0] : [0xe32b50, 1.0]

                const material =
                    materialStyle === MaterialStyle.Regular
                        ? new THREE.MeshPhongMaterial({
                              color: hex,
                              shininess: 0.0,
                              shadowSide: THREE.DoubleSide,
                              opacity: opacity,
                              transparent: opacity < 1.0,
                          })
                        : materialStyle === MaterialStyle.Normals
                          ? new THREE.MeshNormalMaterial()
                          : World.SceneRenderer.CreateToonMaterial(hex, 5)

                World.SceneRenderer.SetupMaterial(material)
                this._materials.set(appearanceId, material)
            }
        )
    }

    /**
     * Creates ThreeJS meshes from the parsed mirabuf file.
     */
    private CreateMeshes() {
        const assembly = this._mirabufParser.assembly
        const instances = assembly.data!.parts!.partInstances!

        interface BatchCounts {
            maxInstances: number
            maxVertices: number
            maxIndices: number
        }

        const batchMap = new Map<THREE.Material, Map<string, [mirabuf.IBody, Array<mirabuf.IPartInstance>]>>()
        const countMap = new Map<THREE.Material, BatchCounts>()
        // Filter all instances by first material, then body
        Object.values(instances).forEach(instance => {
            const definition = assembly.data!.parts!.partDefinitions![instance.partDefinitionReference!]
            const bodies = definition?.bodies ?? []
            bodies.forEach(body => {
                const mesh = body?.triangleMesh?.mesh
                if (!mesh?.verts || !mesh.normals || !mesh.uv || !mesh.indices) return

                const appearanceOverride = body.appearanceOverride
                const material: THREE.Material = WIREFRAME
                    ? new THREE.MeshStandardMaterial({ wireframe: true, color: 0x000000 })
                    : appearanceOverride && this._materials.has(appearanceOverride)
                      ? this._materials.get(appearanceOverride)!
                      : fillerMaterials[nextFillerMaterial++ % fillerMaterials.length]

                let materialBodyMap = batchMap.get(material)
                if (!materialBodyMap) {
                    materialBodyMap = new Map<string, [mirabuf.IBody, Array<mirabuf.IPartInstance>]>()
                    batchMap.set(material, materialBodyMap)
                }

                const partBodyGuid = this.GetPartBodyGuid(definition, body)
                let bodyInstances = materialBodyMap.get(partBodyGuid)
                if (!bodyInstances) {
                    bodyInstances = [body, new Array<mirabuf.IPartInstance>()]
                    materialBodyMap.set(partBodyGuid, bodyInstances)
                }
                bodyInstances[1].push(instance)

                if (countMap.has(material)) {
                    const count = countMap.get(material)!
                    count.maxInstances += 1
                    count.maxVertices += mesh.verts.length / 3
                    count.maxIndices += mesh.indices.length
                    return
                }

                const count: BatchCounts = {
                    maxInstances: 1,
                    maxVertices: mesh.verts.length / 3,
                    maxIndices: mesh.indices.length,
                }
                countMap.set(material, count)
            })
        })

        console.debug(batchMap)

        // Construct batched meshes
        batchMap.forEach((materialBodyMap, material) => {
            const count = countMap.get(material)!
            const batchedMesh = new THREE.BatchedMesh(count.maxInstances, count.maxVertices, count.maxIndices)
            this._batches.push(batchedMesh)

            batchedMesh.material = material
            batchedMesh.castShadow = true
            batchedMesh.receiveShadow = true

            materialBodyMap.forEach(instances => {
                const body = instances[0]
                instances[1].forEach(instance => {
                    const mat = this._mirabufParser.globalTransforms.get(instance.info!.GUID!)!

                    const geometry = new THREE.BufferGeometry()
                    transformGeometry(geometry, body.triangleMesh!.mesh!)
                    const geoId = batchedMesh.addGeometry(geometry)

                    batchedMesh.setMatrixAt(geoId, mat)

                    let bodies = this._meshes.get(instance.info!.GUID!)
                    if (!bodies) {
                        bodies = new Array<[THREE.BatchedMesh, number]>()
                        this._meshes.set(instance.info!.GUID!, bodies)
                    }

                    bodies.push([batchedMesh, geoId])
                })
            })
        })
    }

    private GetPartBodyGuid(partDef: mirabuf.IPartDefinition, body: mirabuf.IPartDefinition) {
        return `${partDef.info!.GUID!}_BODY_${body.info!.GUID!}`
    }

    /**
     * Adds all the meshes to the ThreeJs scene.
     *
     * @param scene
     */
    public AddToScene(scene: THREE.Scene) {
        this._batches.forEach(x => scene.add(x))
    }

    /**
     * Disposes of all ThreeJs scenes and materials.
     */
    public Dispose(scene: THREE.Scene) {
        this._batches.forEach(x => {
            x.dispose()
            scene.remove(x)
        })
        this._batches = []
        this._meshes.clear()

        this._materials.forEach(x => x.dispose())
        this._materials.clear()
    }
}

export default MirabufInstance
