import * as THREE from "three"
import { mirabuf } from "@/proto/mirabuf"
import type { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import { convertMirabufTransformToThreeMatrix } from "@/util/TypeConversions"

export type RigidNodeId = string

export enum ParseErrorSeverity {
    UNIMPORTABLE = 10,
    LIKELY_ISSUES = 6,
    PROBABLY_OKAY = 5,
    JUST_A_WARNING = 2,
}

export const GROUNDED_JOINT_ID = "grounded"
export const GAMEPIECE_SUFFIX = "_gp"

export type ParseError = [severity: ParseErrorSeverity, message: string]

/**
 * TODO:
 * 1. Account for special versions
 * 2. Gamepieces added to their own RigidNodes
 */
class MirabufParser {
    private _nodeNameCounter: number = 0

    private _assembly: mirabuf.Assembly
    private _errors: Array<ParseError>
    private _directedGraph: Graph
    private _rootNode: string

    protected _partTreeValues: Map<string, number> = new Map()
    private _designHierarchyRoot: mirabuf.INode = new mirabuf.Node()

    protected _partToNodeMap: Map<string, RigidNode> = new Map()
    protected _rigidNodes: Array<RigidNode> = []
    private _globalTransforms: Map<string, THREE.Matrix4>

    private _groundedNode: RigidNode | undefined

    private _gamePieces?: MirabufParser[]
    private _isGamePiece: boolean
    private _gamePieceTransform?: mirabuf.ITransform

    public get errors() {
        return [...this._errors]
    }
    public get maxErrorSeverity() {
        return Math.max(...this._errors.map(x => x[0]))
    }
    public get assembly() {
        return this._assembly
    }
    public get partTreeValues() {
        return this._partTreeValues
    }
    public get designHierarchyRoot() {
        return this._designHierarchyRoot
    }
    public get partToNodeMap() {
        return this._partToNodeMap
    }
    public get globalTransforms() {
        return this._globalTransforms
    }
    public get groundedNode() {
        return this._groundedNode ? new RigidNodeReadOnly(this._groundedNode) : undefined
    }
    public get rigidNodes(): Map<RigidNodeId, RigidNodeReadOnly> {
        return new Map(this._rigidNodes.map(x => [x.id, new RigidNodeReadOnly(x)]))
    }
    public get directedGraph() {
        return this._directedGraph
    }
    public get rootNode() {
        return this._rootNode
    }
    public get gamePieces(): MirabufParser[] | undefined {
        return this._gamePieces
    }
    public get isGamePiece(): boolean {
        return this._isGamePiece
    }
    public get gamePieceTransform(): mirabuf.ITransform | undefined {
        return this._gamePieceTransform
    }

    public constructor(assembly: mirabuf.Assembly, isGamePiece: boolean = false, progressHandle?: ProgressHandle) {
        this._assembly = assembly
        this._errors = []
        this._globalTransforms = new Map()
        this._gamePieces = undefined
        this._isGamePiece = isGamePiece
        if (isGamePiece && assembly.transform) this._gamePieceTransform = assembly.transform

        progressHandle?.update("Parsing assembly...", 0.3)

        this.generateTreeValues()
        this.loadGlobalTransforms()

        this.initializeRigidGroups() // 1: from ancestral breaks in joints

        // Fields Only: Assign Game Piece rigid nodes
        if (!assembly.dynamic) {
            progressHandle?.update("Wrangling Gamepieces...", 0.4)
            this._gamePieces = this.pruneGamePieceNodes().map(gp => new MirabufParser(gp, true))
        }

        // 2: Grounded joint
        const gInst = assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        const gNode = this.newRigidNode()
        this.movePartToRigidNode(gInst.parts!.nodes!.at(0)!.value!, gNode)

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 3: Traverse and round up
        const traverseNodeRoundup = (node: mirabuf.INode, parentNode: RigidNode) => {
            const currentNode = this._partToNodeMap.get(node.value!)
            if (!currentNode) this.movePartToRigidNode(node.value!, parentNode)

            if (!node.children) return
            node.children.forEach(x => traverseNodeRoundup(x, currentNode ?? parentNode))
        }
        this._designHierarchyRoot.children?.forEach(x => traverseNodeRoundup(x, gNode))

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        this.bandageRigidNodes(assembly) // 4: Bandage via RigidGroups
        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 5. Remove Empty RNs
        this._rigidNodes = this._rigidNodes.filter(x => x.parts.size > 0)

        // A parser representing a standalone game piece asset never runs pruneGamePieceNodes
        // on itself, so its own rigid nodes need to be flagged directly.
        if (this._isGamePiece) {
            this._rigidNodes.forEach(rn => {
                rn.isGamePiece = true
            })
        }

        // 6. If field, find grounded node and set isDynamic to false. Also just find grounded node again
        this._groundedNode = this.partToNodeMap.get(gInst.parts!.nodes!.at(0)!.value!)
        if (!assembly.dynamic && this._groundedNode) this._groundedNode.isDynamic = false

        // 7. Update root RigidNode
        const rootNodeId = this._partToNodeMap.get(gInst.parts!.nodes!.at(0)!.value!)?.id ?? this._rigidNodes[0].id
        this._rootNode = rootNodeId

        // 8. Retrieve Masses
        this._rigidNodes.forEach(rn => {
            rn.mass = 0
            rn.parts.forEach(part => {
                const inst = assembly.data?.parts?.partInstances?.[part]
                if (!inst?.partDefinitionReference) return
                const def = assembly.data?.parts?.partDefinitions?.[inst.partDefinitionReference!]
                rn.mass += def?.massOverride ? def.massOverride : (def?.physicalData?.mass ?? 0)
            })
        })

        this._directedGraph = this.generateRigidNodeGraph(assembly, rootNodeId)

        if (!this.assembly.data?.parts?.partDefinitions) {
            console.warn("Failed to get part definitions")
            return
        }
    }

    private traverseTree(nodes: mirabuf.INode[], op: (node: mirabuf.INode) => void) {
        nodes.forEach(node => {
            if (node.children) this.traverseTree(node.children, op)
            op(node)
        })
    }

    private initializeRigidGroups() {
        const jointInstanceKeys = Object.keys(this._assembly.data!.joints!.jointInstances!) as string[]
        jointInstanceKeys.forEach(key => {
            if (key === GROUNDED_JOINT_ID) return

            const jInst = this._assembly.data!.joints!.jointInstances![key]
            const [ancestorA, ancestorB] = this.findAncestralBreak(jInst.parentPart!, jInst.childPart!)
            const parentRN = this.newRigidNode()

            this.movePartToRigidNode(ancestorA, parentRN)
            this.movePartToRigidNode(ancestorB, this.newRigidNode())

            if (jInst.parts && jInst.parts.nodes)
                this.traverseTree(jInst.parts.nodes, x => this.movePartToRigidNode(x.value!, parentRN))
        })
    }

    /*
     * Separates and returns the sub-assemblies (partInstances) of each game piece
     */
    private pruneGamePieceNodes(): mirabuf.Assembly[] {
        // Collect all definitions labeled as gamepieces (dynamic = true)
        const gamepieceDefinitions: Set<string> = new Set(
            Object.values(this._assembly.data!.parts!.partDefinitions!)
                .filter(def => def.dynamic)
                .map((def: mirabuf.IPartDefinition) => def.info!.GUID!)
        )

        // Create gamepiece rigid nodes from PartInstances with corresponding definitions
        const gamePieces = Object.values(this._assembly.data!.parts!.partInstances!)
            .filter(inst => gamepieceDefinitions.has(inst.partDefinitionReference!))
            .map(inst => {
                const instNode = this.binarySearchDesignTree(inst.info!.GUID!)
                if (instNode == null) {
                    this.NewError(
                        ParseErrorSeverity.LIKELY_ISSUES,
                        `Failed to find game piece in Design Tree: GUID='${inst.info!.GUID}' name='${inst.info!.name}'`
                    )
                    return
                }
                // Trick to capture and delete references to gamePiece
                // Removing this yields a null function runtime error
                const gpRn = this.newRigidNode(GAMEPIECE_SUFFIX)
                gpRn.isGamePiece = true
                this.movePartToRigidNode(instNode!.value!, gpRn)
                if (instNode.children)
                    this.traverseTree(instNode.children, x => this.movePartToRigidNode(x.value!, gpRn))
                // Includes descendants (e.g. a collision sub-part) so their data carries over too
                const allParts = [...gpRn.parts]
                this.deleteRigidNode(gpRn)

                // Delete partInstances
                Object.entries(this._assembly.data?.parts?.partInstances ?? {})
                    .filter(([_key, subInst]) => inst === subInst)
                    .forEach(([key, _subInst]) => delete this._assembly.data?.parts?.partInstances?.[key])

                // Assumes that the game piece is composed of one instance
                const worldTransform = this._globalTransforms.get(inst.info!.GUID!)
                return this.convertPartInstanceToAssembly(inst, instNode, allParts, false, true, worldTransform)
            })
            .filter(asm => asm != undefined)

        return gamePieces
    }

    /*
     * Converts specfic part instances to entire assemblies. Designed and tested for gamePiece instances, but theoretically should generalize
     *
     * Assumptions:
     * - The part is a single dynamic rigid body
     * - One joint and one part exist on the instance
     */
    private convertPartInstanceToAssembly(
        inst: mirabuf.IPartInstance,
        instNode: mirabuf.INode,
        allParts: string[],
        isEndEffector: boolean = false,
        isDynamic: boolean = true,
        worldTransform?: THREE.Matrix4
    ): mirabuf.Assembly | undefined {
        const jointDefinition = new mirabuf.joint.Joint({
            info: {
                GUID: GROUNDED_JOINT_ID,
                name: GROUNDED_JOINT_ID,
            },
            jointMotionType: mirabuf.joint.JointMotion.RIGID,
            // Affects the placement of the joints, cannot affect the placement of the assembly in absolute space
            origin: new mirabuf.Vector3(),
        })
        const jointInstance = new mirabuf.joint.JointInstance({
            isEndEffector,
            parentPart: "",
            jointReference: jointDefinition.info?.GUID,
            parts: { nodes: [instNode] },
        })

        const joints = new mirabuf.joint.Joints({
            jointDefinitions: {
                [GROUNDED_JOINT_ID]: jointDefinition,
            },
            jointInstances: {
                [GROUNDED_JOINT_ID]: jointInstance,
            },
            rigidGroups: [],
            // This probably needs to be changed if this function gets generalized for mix-n-match or something
            motorDefinitions: {},
        })

        const partDefinitionReference = inst?.partDefinitionReference
        if (partDefinitionReference == null) {
            this.NewError(ParseErrorSeverity.UNIMPORTABLE, "partInstance does not reference a partDefinition")
            return
        }

        // Register every part in allParts, not just inst, in case of multi-part game pieces
        const partInstances: { [guid: string]: mirabuf.IPartInstance } = {}
        const partDefinitions: { [guid: string]: mirabuf.IPartDefinition } = {}
        allParts.forEach(partGUID => {
            const partInst = this._assembly.data?.parts?.partInstances?.[partGUID]
            if (!partInst?.partDefinitionReference) return
            partInstances[partGUID] = partInst
            partDefinitions[partInst.partDefinitionReference] =
                this._assembly.data?.parts?.partDefinitions?.[partInst.partDefinitionReference] ?? {}
        })
        partInstances[inst.info?.GUID ?? ""] = inst
        partDefinitions[partDefinitionReference] =
            this.assembly.data?.parts?.partDefinitions?.[partDefinitionReference] ?? {}

        const parts = new mirabuf.Parts({
            info: inst.info,
            partDefinitions,
            partInstances,
        })

        const gamePieceAssembly = new mirabuf.Assembly({
            info: inst.info,
            data: {
                parts,
                joints,
                materials: this.assembly.data?.materials,
                // This probably needs to be changed if this function gets generalized for mix-n-match or something
                signals: {},
            },
            dynamic: isDynamic,
            designHierarchy: { nodes: [instNode] },
            // This probably needs to be changed if this function gets generalized for mix-n-match or something
            jointHierarchy: {},
            thumbnail: null,
            transform: worldTransform
                ? (() => {
                      const e = worldTransform.elements
                      return new mirabuf.Transform({
                          // biome-ignore-start format: We would prefer to visualize this as a matrix
                          spatialMatrix: [
                              e[0], e[4], e[8],  e[12] * 100,
                              e[1], e[5], e[9],  e[13] * 100,
                              e[2], e[6], e[10], e[14] * 100,
                              e[3], e[7], e[11], e[15],
                          ],
                          // biome-ignore-end format: We would prefer to visualize this as a matrix
                      })
                  })()
                : inst.transform,
        })

        return gamePieceAssembly
    }

    private bandageRigidNodes(assembly: mirabuf.Assembly) {
        assembly.data!.joints!.rigidGroups!.forEach(rg => {
            let rn: RigidNode | null = null
            rg.occurrences!.forEach(y => {
                // Occurrence may have been extracted as a game piece, leaving no rigid node
                const currentRn = this._partToNodeMap.get(y)
                if (!currentRn) return

                rn = !rn ? currentRn : currentRn.id != rn.id ? this.mergeRigidNodes(currentRn, rn) : rn
            })
        })
    }

    private generateRigidNodeGraph(assembly: mirabuf.Assembly, rootNodeId: string): Graph {
        // Build undirected graph
        const graph = new Graph()
        graph.addNode(rootNodeId)
        const jointInstances = Object.values(assembly.data!.joints!.jointInstances!) as mirabuf.joint.JointInstance[]
        jointInstances.forEach((x: mirabuf.joint.JointInstance) => {
            const rA = this._partToNodeMap.get(x.parentPart)
            const rB = this._partToNodeMap.get(x.childPart)

            if (!rA || !rB || rA.id == rB.id) return
            graph.addNode(rA.id)
            graph.addNode(rB.id)
            graph.addEdgeUndirected(rA.id, rB.id)
        })

        const directedGraph = new Graph()
        const whiteGreyBlackMap = new Map<string, boolean>()
        this._rigidNodes.forEach(node => {
            whiteGreyBlackMap.set(node.id, false)
            directedGraph.addNode(node.id)
        })

        const directedRecursive = (node: string) => {
            graph
                .getAdjacencyList(node)
                .filter(x => whiteGreyBlackMap.has(x))
                .forEach(x => {
                    directedGraph.addEdgeDirected(node, x)
                    whiteGreyBlackMap.delete(x)
                    directedRecursive(x)
                })
        }

        whiteGreyBlackMap.delete(rootNodeId)
        directedRecursive(rootNodeId)

        return directedGraph
    }

    private newRigidNode(suffix?: string): RigidNode {
        const node = new RigidNode(`${this._nodeNameCounter++}${suffix ?? ""}`)
        this._rigidNodes.push(node)
        return node
    }

    private deleteRigidNode(node: RigidNode) {
        const index = this._rigidNodes.indexOf(node)
        if (index != -1 && index != null) {
            this._rigidNodes.splice(index)
        }
    }

    private mergeRigidNodes(rnA: RigidNode, rnB: RigidNode) {
        const newRn = this.newRigidNode("merged")
        const allParts = new Set<string>([...rnA.parts, ...rnB.parts])
        allParts.forEach(x => this.movePartToRigidNode(x, newRn))
        return newRn
    }

    private movePartToRigidNode(part: string, node: RigidNode) {
        if (part.length < 1) return

        const original = this._partToNodeMap.get(part)
        if (original) {
            if (original === node) return

            original.parts.delete(part)
            this._partToNodeMap.delete(part)
        }

        node.parts.add(part)
        this._partToNodeMap.set(part, node)
    }

    /**
     * Loads this._globalTransforms with the world space transformations of each part instance.
     */
    private loadGlobalTransforms() {
        const root = this._designHierarchyRoot
        const partInstances = new Map<string, mirabuf.IPartInstance>(
            Object.entries(this._assembly.data!.parts!.partInstances!)
        )
        const partDefinitions = this._assembly.data!.parts!.partDefinitions!

        this._globalTransforms.clear()

        const getTransforms = (node: mirabuf.INode, parent: THREE.Matrix4) => {
            node.children!.forEach(child => {
                const partInstance: mirabuf.IPartInstance | undefined = partInstances.get(child.value!)

                if (!partInstance || this.globalTransforms.has(child.value!)) return
                const mat = convertMirabufTransformToThreeMatrix(partInstance.transform!)!

                // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat)}`);

                this._globalTransforms.set(child.value!, mat.premultiply(parent))
                getTransforms(child, mat)
            })
        }

        root.children?.forEach(child => {
            const partInstance = partInstances.get(child.value!)!
            const def = partDefinitions[partInstance.partDefinitionReference!]

            // gamePieceTransform is already the resolved world transform; partInstance.transform is only local
            let mat: THREE.Matrix4
            if (this._isGamePiece && this._gamePieceTransform) {
                mat = convertMirabufTransformToThreeMatrix(this._gamePieceTransform)
            } else if (partInstance.transform) {
                mat = convertMirabufTransformToThreeMatrix(partInstance.transform)
            } else if (def.baseTransform) {
                mat = convertMirabufTransformToThreeMatrix(def.baseTransform)
            } else {
                mat = new THREE.Matrix4().identity()
            }

            // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat!)}`);

            this._globalTransforms.set(partInstance.info!.GUID!, mat)
            getTransforms(child, mat)
        })
    }

    private findAncestralBreak(partA: string, partB: string): [string, string] {
        if (!this._partTreeValues.has(partA) || !this._partTreeValues.has(partB)) {
            this._errors.push([ParseErrorSeverity.LIKELY_ISSUES, "Part not found in tree."])
            return [partA, partB]
        } else if (partA == partB) {
            this._errors.push([ParseErrorSeverity.LIKELY_ISSUES, "Part A and B are the same."])
        }

        const ptv = this._partTreeValues
        let pathA = this._designHierarchyRoot
        let pathB = this._designHierarchyRoot
        const valueA = ptv.get(partA)!
        const valueB = ptv.get(partB)!

        while (pathA.value! == pathB.value! && pathA.value! != partA && pathB.value! != partB) {
            const ancestorIndexA = this.binarySearchIndex(valueA, pathA.children!)
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)]

            const ancestorIndexB = this.binarySearchIndex(valueB, pathB.children!)
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)]
        }

        if (pathA.value! == partA && pathA.value! == pathB.value!) {
            const ancestorIndexB = this.binarySearchIndex(valueB, pathB.children!)
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)]
        } else if (pathB.value! == partB && pathA.value! == pathB.value!) {
            const ancestorIndexA = this.binarySearchIndex(valueA, pathA.children!)
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)]
        }

        return [pathA.value!, pathB.value!]
    }

    private binarySearchIndex(target: number, children: mirabuf.INode[]): number {
        let l = 0
        let h = children.length

        while (h - l > 1) {
            const i = Math.floor((h + l) / 2.0)
            const iVal = this._partTreeValues.get(children[i].value!)!
            if (iVal > target) {
                h = i
            } else if (iVal < target) {
                l = i + 1
            } else {
                return i
            }
        }

        return Math.floor((h + l) / 2.0)
    }

    private binarySearchDesignTree(target: string): mirabuf.INode | null {
        let node = this._designHierarchyRoot
        const targetValue = this._partTreeValues.get(target)!

        while (node?.value != target && node?.children) {
            const i = this.binarySearchIndex(targetValue, node.children!)
            const iValue = this._partTreeValues.get(node.children![i].value!)!
            node = node.children![i + (iValue < targetValue ? 1 : 0)]
        }

        return node?.value === target ? node : null
    }

    private generateTreeValues() {
        let nextValue = 0
        const partTreeValues = new Map<string, number>()

        const recursive = (partNode: mirabuf.INode) => {
            partNode.children = partNode.children?.filter(x => x.value != null)
            partNode.children?.forEach(x => recursive(x))
            partTreeValues.set(partNode.value!, nextValue++)
        }

        this._designHierarchyRoot = new mirabuf.Node()
        this._designHierarchyRoot.value = "Importer Generated Root"
        this._designHierarchyRoot.children = []
        this._designHierarchyRoot.children.push(...this._assembly.designHierarchy!.nodes!)

        recursive(this._designHierarchyRoot)
        this._partTreeValues = partTreeValues
    }

    private NewError(severity: ParseErrorSeverity, message: string) {
        if (severity >= ParseErrorSeverity.LIKELY_ISSUES) {
            console.error(message)
            if (severity == ParseErrorSeverity.UNIMPORTABLE)
                console.error(`Aborting Parse of assembly: ${this._assembly.info?.name}`)
        } else {
            console.warn(message)
        }
        this._errors.push([severity, message])
    }
}

export function zeroGamePieceInstancePosition(assembly: mirabuf.Assembly) {
    const partInstances = assembly.data?.parts?.partInstances
    const instance = partInstances ? Object.values(partInstances)[0] : undefined
    if (!instance?.transform) return

    const pos = new THREE.Vector3()
    const quat = new THREE.Quaternion()
    const scale = new THREE.Vector3()
    convertMirabufTransformToThreeMatrix(instance.transform).decompose(pos, quat, scale)

    const zeroed = new THREE.Matrix4().compose(new THREE.Vector3(0, 0, 0), quat, scale)
    const e = zeroed.elements
    instance.transform = new mirabuf.Transform({
        // biome-ignore-start format: We would prefer to visualize this as a matrix
        spatialMatrix: [
            e[0], e[4], e[8],  0,
            e[1], e[5], e[9],  0,
            e[2], e[6], e[10], 0,
            e[3], e[7], e[11], e[15],
        ],
        // biome-ignore-end format: We would prefer to visualize this as a matrix
    })
}

/**
 * Collection of mirabuf parts that are bound together
 */
class RigidNode {
    public id: RigidNodeId
    public parts: Set<string> = new Set()
    public isDynamic: boolean
    public isGamePiece: boolean
    public mass: number

    public constructor(id: RigidNodeId, isDynamic?: boolean, isGamePiece?: boolean, mass?: number) {
        this.id = id
        this.isDynamic = isDynamic ?? true
        this.isGamePiece = isGamePiece ?? false
        this.mass = mass ?? 0
    }
}

export class RigidNodeReadOnly {
    private _original: RigidNode

    public get id(): RigidNodeId {
        return this._original.id
    }

    public get parts(): ReadonlySet<string> {
        return this._original.parts
    }

    public get isDynamic(): boolean {
        return this._original.isDynamic
    }

    public get isGamePiece(): boolean {
        return this._original.isGamePiece
    }

    public get mass(): number {
        return this._original.mass
    }

    public constructor(original: RigidNode) {
        this._original = original
    }
}

export class Graph {
    private _adjacencyMap: Map<string, string[]>

    public get nodes() {
        return this._adjacencyMap.keys()
    }

    public constructor() {
        this._adjacencyMap = new Map()
    }

    public addNode(node: string) {
        if (!this._adjacencyMap.has(node)) this._adjacencyMap.set(node, [])
    }

    public addEdgeUndirected(nodeA: string, nodeB: string) {
        if (!this._adjacencyMap.has(nodeA) || !this._adjacencyMap.has(nodeB)) throw new Error("Nodes aren't in graph")

        this._adjacencyMap.get(nodeA)!.push(nodeB)
        this._adjacencyMap.get(nodeB)!.push(nodeA)
    }

    public addEdgeDirected(nodeA: string, nodeB: string) {
        if (!this._adjacencyMap.has(nodeA) || !this._adjacencyMap.has(nodeB)) throw new Error("Nodes aren't in graph")

        this._adjacencyMap.get(nodeA)!.push(nodeB)
    }

    public getAdjacencyList(node: string) {
        if (!this._adjacencyMap.has(node)) {
            // Don't remove this. Without this check initially, Map.get *randomly* fails. I have no clue why...
            throw new Error(`Node '${node}' is not in adjacency list`)
        }
        return this._adjacencyMap.get(node)!
    }
}

export default MirabufParser
