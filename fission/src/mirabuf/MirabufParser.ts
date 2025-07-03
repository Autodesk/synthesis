import * as THREE from "three"
import { mirabuf } from "@/proto/mirabuf"
import { MirabufTransform_ThreeMatrix4 } from "@/util/TypeConversions"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import { randomUUID } from "crypto"
import { join } from "path"

export type RigidNodeId = string

export enum ParseErrorSeverity {
    Unimportable = 10,
    LikelyIssues = 6,
    ProbablyOkay = 5,
    JustAWarning = 2,
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

    public get errors() {
        return this._errors
    }

    public get maxErrorSeverity(): number {
        return Math.max(...this._errors.map(x => x[0]))
    }
    public get assembly(): mirabuf.Assembly {
        return this._assembly
    }
    public get partTreeValues(): Map<string, number> {
        return this._partTreeValues
    }
    public get designHierarchyRoot(): mirabuf.INode {
        return this._designHierarchyRoot
    }
    public get partToNodeMap(): Map<string, RigidNode> {
        return this._partToNodeMap
    }
    public get globalTransforms(): Map<string, THREE.Matrix4> {
        return this._globalTransforms
    }
    public get groundedNode(): RigidNodeReadOnly | undefined {
        return this._groundedNode ? new RigidNodeReadOnly(this._groundedNode) : undefined
    }
    public get rigidNodes(): Map<RigidNodeId, RigidNodeReadOnly> {
        return new Map(this._rigidNodes.map(x => [x.id, new RigidNodeReadOnly(x)]))
    }
    public get directedGraph(): Graph {
        return this._directedGraph
    }
    public get rootNode(): string {
        return this._rootNode
    }
    public get gamePieces(): MirabufParser[] | undefined {
        return this._gamePieces
    }
    public get isGamePiece(): boolean {
        return this._isGamePiece
    }

    public constructor(assembly: mirabuf.Assembly, isGamePiece: boolean = false, progressHandle?: ProgressHandle) {
        this._assembly = assembly
        this._errors = new Array<ParseError>()
        this._globalTransforms = new Map()
        this._gamePieces = undefined
        this._isGamePiece = isGamePiece

        progressHandle?.Update("Parsing assembly...", 0.3)

        this.GenerateTreeValues()
        this.LoadGlobalTransforms()

        this.InitializeRigidGroups() // 1: from ancestral breaks in joints

        // Fields Only: Assign Game Piece rigid nodes
        if (!assembly.dynamic) {
            this._gamePieces = this.PruneGamePieceNodes().map(assembly => new MirabufParser(assembly, true))
        }

        // 2: Grounded joint
        const gInst = assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID]
        const gNode = this.NewRigidNode()
        this.MovePartToRigidNode(gInst.parts!.nodes!.at(0)!.value!, gNode)

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 3: Traverse and round up
        const traverseNodeRoundup = (node: mirabuf.INode, parentNode: RigidNode) => {
            const currentNode = this._partToNodeMap.get(node.value!)
            if (!currentNode) this.MovePartToRigidNode(node.value!, parentNode)

            if (!node.children) return
            node.children.forEach(x => traverseNodeRoundup(x, currentNode ?? parentNode))
        }
        this._designHierarchyRoot.children?.forEach(x => traverseNodeRoundup(x, gNode))

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        this.BandageRigidNodes(assembly) // 4: Bandage via RigidGroups
        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 5. Remove Empty RNs
        this._rigidNodes = this._rigidNodes.filter(x => x.parts.size > 0)

        // 6. If field, find grounded node and set isDynamic to false. Also just find grounded node again
        this._groundedNode = this.partToNodeMap.get(gInst.parts!.nodes!.at(0)!.value!)
        if (!assembly.dynamic && this._groundedNode) this._groundedNode.isDynamic = false

        // 7. Update root RigidNode
        const rootNodeId = this._partToNodeMap.get(gInst.parts!.nodes!.at(0)!.value!)?.id ?? this._rigidNodes[0].id
        this._rootNode = rootNodeId

        // 8. Retrieve Masses
        this._rigidNodes.forEach(rn => {
            rn.mass = [...rn.parts]
                .map(part => assembly.data?.parts?.partInstances?.[part])
                .filter(inst => inst?.partDefinitionReference)
                .reduce<number>((acc, inst) => {
                    const def = assembly.data?.parts?.partDefinitions?.[inst?.partDefinitionReference!]
                    return acc + (def?.massOverride ?? def?.physicalData?.mass ?? 0)
                }, 0)
        })

        this._directedGraph = this.GenerateRigidNodeGraph(assembly, rootNodeId)

        if (!this.assembly.data?.parts?.partDefinitions) console.warn("Failed to get part definitions")
    }

    private TraverseTree(nodes: mirabuf.INode[], op: (node: mirabuf.INode) => void) {
        nodes.forEach(node => {
            if (node.children) this.TraverseTree(node.children, op)
            op(node)
        })
    }

    private InitializeRigidGroups() {
        const jointInstanceKeys = Object.keys(this._assembly.data!.joints!.jointInstances!) as string[]
        jointInstanceKeys
            .filter(key => key !== GROUNDED_JOINT_ID)
            .forEach(key => {
                const jInst = this._assembly.data!.joints!.jointInstances![key]
                const [ancestorA, ancestorB] = this.FindAncestorialBreak(jInst.parentPart!, jInst.childPart!)
                const parentRN = this.NewRigidNode()

                this.MovePartToRigidNode(ancestorA, parentRN)
                this.MovePartToRigidNode(ancestorB, this.NewRigidNode())

                if (jInst.parts && jInst.parts.nodes)
                    this.TraverseTree(jInst.parts.nodes, x => this.MovePartToRigidNode(x.value!, parentRN))
            })
    }

    // Separates and returns the sub-asmeblies (partInstances) of each game piece
    private PruneGamePieceNodes(): mirabuf.Assembly[] {
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
                // To fix the issue where some game  pieces are child nodes of others, iteratively search the designHierarchy then all the previously pruned gamepieces trees for the current node
                const instNode = this.BinarySearchDesignTreePrune(inst.info!.GUID!)
                if (instNode == null) {
                    console.error("Failed to find Game piece in Design Tree")
                    this._errors.push([ParseErrorSeverity.LikelyIssues, "Failed to find Game piece in Design Tree"])
                    return
                }
                // TODO: Instead of marking them, separate them into a different body entirely
                // Figure out what we actually need to return here

                // Trick to capture and delete references to gamePiece
                // const gpRn = this.NewRigidNode(GAMEPIECE_SUFFIX)
                // gpRn.isGamePiece = true
                // this.MovePartToRigidNode(instNode!.value!, gpRn)
                // if (instNode.children)
                //     this.TraverseTree(instNode.children, x => this.MovePartToRigidNode(x.value!, gpRn))
                // this.DeleteRigidNode(gpRn)

                // Delete partInstances
                Object.entries(this._assembly.data?.parts?.partInstances!)
                    .filter(([_key, subInst]) => inst === subInst)
                    .forEach(([key, _subInst]) => delete this._assembly.data?.parts?.partInstances?.[key])

                // Delete partDefinitions
                // Object.entries(this._assembly.data?.parts?.partDefinitions!)
                //     .filter(([_key, subInst]) => inst === subInst)
                //     .forEach(([key, _subInst]) => delete this._assembly.data?.parts?.partDefinitions?.[key])

                return this.PartInstance_Assembly(inst, instNode)
            })
            .filter(asm => asm != null)

        // TODO: Detatch game pieces from tree and remove part instances
        console.log(`${gamePieces.length}`)

        return gamePieces
    }

    private PartInstance_Assembly(inst: mirabuf.IPartInstance, instNode: mirabuf.INode): mirabuf.Assembly {
        // Create grounded joint
        const jointDefinition = new mirabuf.joint.Joint({
            info: {
                GUID: GROUNDED_JOINT_ID,
                name: "grounded",
            },
            jointMotionType: mirabuf.joint.JointMotion.RIGID,
            origin: new mirabuf.Vector3(),
        })
        const jointInstance = new mirabuf.joint.JointInstance({
            isEndEffector: false,
            parentPart: "",
            jointReference: jointDefinition.info?.name,
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
            motorDefinitions: {},
        })

        const partDefinitionReference = inst?.partDefinitionReference ?? ""
        const partDefinition = this.assembly.data?.parts?.partDefinitions?.[partDefinitionReference] ?? {}

        const parts = new mirabuf.Parts({
            info: inst.info,
            partDefinitions: {
                [partDefinitionReference]: partDefinition,
            },
            partInstances: {
                [inst.info?.GUID ?? ""]: inst,
            },
        })

        console.log(inst.info?.name)
        const gamePieceAssembly = new mirabuf.Assembly({
            info: inst.info,
            data: {
                parts,
                joints,
                materials: this.assembly.data?.materials,
                signals: {},
            },
            dynamic: true,
            designHierarchy: { nodes: [instNode] },
            jointHierarchy: {},
            thumbnail: null,
        })

        return gamePieceAssembly
    }

    private BandageRigidNodes(assembly: mirabuf.Assembly) {
        assembly.data!.joints!.rigidGroups!.forEach(rg => {
            let rn: RigidNode | null = rg.occurrences!.reduce<RigidNode | null>((rn, y) => {
                const currentRn = this._partToNodeMap.get(y)!

                return !rn ? currentRn : currentRn.id != rn.id ? this.MergeRigidNodes(currentRn, rn) : rn
            }, null)
        })
    }

    private GenerateRigidNodeGraph(assembly: mirabuf.Assembly, rootNodeId: string): Graph {
        // Build undirected graph
        const graph = new Graph()
        graph.AddNode(rootNodeId)
        const jointInstances = Object.values(assembly.data!.joints!.jointInstances!) as mirabuf.joint.JointInstance[]
        jointInstances.forEach((x: mirabuf.joint.JointInstance) => {
            const rA = this._partToNodeMap.get(x.parentPart)
            const rB = this._partToNodeMap.get(x.childPart)

            if (!rA || !rB || rA.id == rB.id) return
            graph.AddNode(rA.id)
            graph.AddNode(rB.id)
            graph.AddEdgeUndirected(rA.id, rB.id)
        })

        const directedGraph = new Graph()
        const whiteGreyBlackMap = new Map<string, boolean>()
        this._rigidNodes.forEach(node => {
            whiteGreyBlackMap.set(node.id, false)
            directedGraph.AddNode(node.id)
        })

        const directedRecursive = (node: string) => {
            graph
                .GetAdjacencyList(node)
                .filter(x => whiteGreyBlackMap.has(x))
                .forEach(x => {
                    directedGraph.AddEdgeDirected(node, x)
                    whiteGreyBlackMap.delete(x)
                    directedRecursive(x)
                })
        }

        whiteGreyBlackMap.delete(rootNodeId)
        directedRecursive(rootNodeId)

        return directedGraph
    }

    private NewRigidNode(suffix?: string): RigidNode {
        const node = new RigidNode(`${this._nodeNameCounter++}${suffix ?? ""}`)
        this._rigidNodes.push(node)
        return node
    }

    private DeleteRigidNode(node: RigidNode) {
        const index = this._rigidNodes.indexOf(node)
        if (index != -1 && index != null) {
            this._rigidNodes.splice(index)
        }
    }

    private MergeRigidNodes(rnA: RigidNode, rnB: RigidNode) {
        const newRn = this.NewRigidNode("merged")
        const allParts = new Set<string>([...rnA.parts, ...rnB.parts])
        allParts.forEach(x => this.MovePartToRigidNode(x, newRn))
        return newRn
    }

    private MovePartToRigidNode(part: string, node: RigidNode) {
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
    private LoadGlobalTransforms() {
        const root = this._designHierarchyRoot
        const parts = this._assembly.data?.parts
        if (!parts) return // TODO not sure if we should return or provide a default value

        const partInstances = new Map<string, mirabuf.IPartInstance>(Object.entries(parts!.partInstances!))
        const partDefinitions = parts!.partDefinitions!

        this._globalTransforms.clear()

        const getTransforms = (node: mirabuf.INode, parent: THREE.Matrix4) => {
            node.children!.forEach(child => {
                const partInstance: mirabuf.IPartInstance | undefined = partInstances.get(child.value!)

                if (!partInstance || this.globalTransforms.has(child.value!)) return
                const mat = MirabufTransform_ThreeMatrix4(partInstance.transform!)!

                // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat)}`);

                this._globalTransforms.set(child.value!, mat.premultiply(parent))
                getTransforms(child, mat)
            })
        }

        root.children?.forEach(child => {
            const partInstance = partInstances.get(child.value!)!
            const def = partDefinitions[partInstance.partDefinitionReference!]

            const mat = partInstance.transform
                ? MirabufTransform_ThreeMatrix4(partInstance.transform)
                : def.baseTransform
                  ? MirabufTransform_ThreeMatrix4(def.baseTransform)
                  : new THREE.Matrix4().identity()

            // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat!)}`);

            this._globalTransforms.set(partInstance.info!.GUID!, mat)
            getTransforms(child, mat)
        })
    }

    private FindAncestorialBreak(partA: string, partB: string): [string, string] {
        if (!this._partTreeValues.has(partA) || !this._partTreeValues.has(partB)) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, "Part not found in tree."])
            return [partA, partB]
        } else if (partA == partB) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, "Part A and B are the same."])
        }

        const ptv = this._partTreeValues
        let pathA = this._designHierarchyRoot
        let pathB = this._designHierarchyRoot
        const valueA = ptv.get(partA)!
        const valueB = ptv.get(partB)!

        const traverse = (value: number | undefined, path: mirabuf.INode | undefined) => {
            if (!value || !path) return

            const ancestorIndex = this.BinarySearchIndex(value, path.children!)
            const ancestorValue = ptv.get(path.children![ancestorIndex].value!)!
            path = path.children![ancestorIndex + (ancestorValue < value ? 1 : 0)]
        }

        while (pathA.value! == pathB.value! && pathA.value! != partA && pathB.value! != partB) {
            traverse(valueB, pathB)
            traverse(valueA, pathA)
        }

        const [value, path] =
            pathA.value! == partA && pathA.value! == pathB.value!
                ? [valueB, pathB]
                : pathB.value! == partB && pathA.value! == pathB.value!
                  ? [valueA, pathA]
                  : [undefined, undefined]

        traverse(value, path)

        return [pathA.value!, pathB.value!]
    }

    private BinarySearchIndex(target: number, children: mirabuf.INode[]): number {
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

    /**
     * Old functon, replaced with BinarySearchDesignTreePrune, but has potentially useful functionality on its own
     */
    private _BinarySearchDesignTree(target: string): mirabuf.INode | null {
        let node = this._designHierarchyRoot
        const targetValue = this._partTreeValues.get(target)!

        while (node.value != target && node.children) {
            const i = this.BinarySearchIndex(targetValue, node.children!)
            const iValue = this._partTreeValues.get(node.children![i].value!)!
            node = node.children![i + (iValue < targetValue ? 1 : 0)]
        }

        return node.value! == target ? node : null
    }

    private BinarySearchDesignTreePrune(target: string): mirabuf.INode | null {
        let parent = this._designHierarchyRoot
        let node = this._designHierarchyRoot
        const targetValue = this._partTreeValues.get(target)!

        while (node?.value != target && node?.children) {
            const i = this.BinarySearchIndex(targetValue, node.children!)
            const iValue = this._partTreeValues.get(node.children![i].value!)!
            parent = node
            node = node.children![i + (iValue < targetValue ? 1 : 0)]
        }

        if (node?.value! == target) {
            const index = parent?.children?.indexOf(node)
            if (index != -1 && index != null) {
                // parent?.children?.splice(index)
            }
            return node
        }

        return null
    }

    private GenerateTreeValues() {
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

    public AddNode(node: string) {
        if (!this._adjacencyMap.has(node)) this._adjacencyMap.set(node, new Array<string>())
    }

    public AddEdgeUndirected(nodeA: string, nodeB: string) {
        if (!this._adjacencyMap.has(nodeA) || !this._adjacencyMap.has(nodeB)) throw new Error("Nodes aren't in graph")

        this._adjacencyMap.get(nodeA)!.push(nodeB)
        this._adjacencyMap.get(nodeB)!.push(nodeA)
    }

    public AddEdgeDirected(nodeA: string, nodeB: string) {
        if (!this._adjacencyMap.has(nodeA) || !this._adjacencyMap.has(nodeB)) throw new Error("Nodes aren't in graph")

        this._adjacencyMap.get(nodeA)!.push(nodeB)
    }

    public GetAdjacencyList(node: string) {
        if (!this._adjacencyMap.has(node)) {
            // Don't remove this. Without this check initially, Map.get *randomly* fails. I have no clue why...
            throw new Error(`Node '${node}' is not in adjacency list`)
        }
        return this._adjacencyMap.get(node)!
    }
}

export default MirabufParser
