import * as THREE from "three";
import { mirabuf } from "../proto/mirabuf";
import { MirabufTransform_ThreeMatrix4 } from "../util/TypeConversions";

export enum ParseErrorSeverity {
    Unimportable = 10,
    LikelyIssues = 6,
    ProbablyOkay = 5,
    JustAWarning = 2
}

const GROUNDED_JOINT_ID = 'grounded';

export type ParseError = [severity: ParseErrorSeverity, message: string];

/**
 * TODO:
 * 1. Account for special versions
 * 2. Gamepieces added to their own RigidNodes
 */
class MirabufParser {

    private _nodeNameCounter: number = 0;

    private _assembly: mirabuf.Assembly;
    private _errors: Array<ParseError>;

    protected _partTreeValues: Map<string, number> = new Map();
    private _designHierarchyRoot: mirabuf.INode = new mirabuf.Node();

    protected _partToNodeMap: Map<string, RigidNode> = new Map();
    protected _rigidNodes: Array<RigidNode> = [];
    private _globalTransforms: Map<string, THREE.Matrix4>;

    public get errors() { return new Array(...this._errors); }

    public get maxErrorSeverity() { return Math.max(...this._errors.map(x => x[0])); }

    public get assembly() { return this._assembly; }

    public get partTreeValues() { return this._partTreeValues; }

    public get designHierarchyRoot() { return this._designHierarchyRoot; }

    public get partToNodeMap() { return this._partToNodeMap; }

    public get globalTransforms() { return this._globalTransforms; }

    public get rigidNodes(): Array<RigidNodeReadOnly> {
        return this._rigidNodes.map(x => new RigidNodeReadOnly(x));
    }

    public constructor(assembly: mirabuf.Assembly) {
        this._assembly = assembly;
        this._errors = new Array<ParseError>();
        this._globalTransforms = new Map();

        this.GenerateTreeValues();
        this.LoadGlobalTransforms();

        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const that = this;

        function traverseTree(nodes: mirabuf.INode[], op: ((node: mirabuf.INode) => void)) {
            nodes.forEach(x => {
                if (x.children) {
                    traverseTree(x.children, op);
                }
                op(x);
            });
        }

        // 1: Initial Rigidgroups from ancestorial breaks in joints
        (Object.keys(assembly.data!.joints!.jointInstances!) as (string)[]).forEach(key => {
            if (key != GROUNDED_JOINT_ID) {
                const jInst = assembly.data!.joints!.jointInstances![key];
                const [ancestorA, ancestorB] = this.FindAncestorialBreak(jInst.parentPart!, jInst.childPart!);
                const parentRN = this.NewRigidNode();
                this.MovePartToRigidNode(ancestorA, parentRN);
                this.MovePartToRigidNode(ancestorB, this.NewRigidNode());
                if (jInst.parts && jInst.parts.nodes)
                    traverseTree(jInst.parts.nodes, x => this.MovePartToRigidNode(x.value!, parentRN));
            }
        });

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // Fields Only: Assign Game Piece rigid nodes
        if (!assembly.dynamic) {
            // Collect all definitions labelled as gamepieces (dynamic = true)
            const gamepieceDefinitions: Set<string> = new Set();
            (Object.values(assembly.data!.parts!.partDefinitions!) as mirabuf.IPartDefinition[]).forEach(def => {
                if (def.dynamic)
                    gamepieceDefinitions.add(def.info!.GUID!);
            });

            // Create gamepiece rigid nodes from partinstances with corresponding definitons
            (Object.values(assembly.data!.parts!.partInstances!) as mirabuf.IPartInstance[]).forEach(inst => {
                if (gamepieceDefinitions.has(inst.partDefinitionReference!)) {
                    const instNode = this.BinarySearchDesignTree(inst.info!.GUID!);
                    if (instNode) {
                        const gpRn = this.NewRigidNode('gp');
                        this.MovePartToRigidNode(instNode!.value!, gpRn);
                        instNode.children && traverseTree(instNode.children, x => this.MovePartToRigidNode(x.value!, gpRn));
                    } else {
                        this._errors.push([ParseErrorSeverity.LikelyIssues, 'Failed to find Game piece in Design Tree']);
                    }
                }
            });
        }

        // 2: Grounded joint
        const gInst = assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID];
        const gNode = this.NewRigidNode();
        this.MovePartToRigidNode(gInst.parts!.nodes!.at(0)!.value!, gNode);
        
        // traverseTree(gInst.parts!.nodes!, x => (!this._partToNodeMap.has(x.value!)) && this.MovePartToRigidNode(x.value!, gNode));
        
        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 3: Traverse and round up
        const traverseNodeRoundup = (node: mirabuf.INode, parentNode: RigidNode) => {
            const currentNode = that._partToNodeMap.get(node.value!);
            if (!currentNode) {
                that.MovePartToRigidNode(node.value!, parentNode);
            }

            if (node.children) {
                node.children!.forEach(x => traverseNodeRoundup(x, currentNode ? currentNode : parentNode));
            }
        }
        this._designHierarchyRoot.children?.forEach(x => traverseNodeRoundup(x, gNode));

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 4: Bandage via rigidgroups
        assembly.data!.joints!.rigidGroups!.forEach(rg => {
            let rn: RigidNode | null = null;
            rg.occurrences!.forEach(y => {
                const currentRn = this._partToNodeMap.get(y)!;
                if (!rn) {
                    rn = currentRn;
                } else if (currentRn.name != rn.name) {
                    rn = this.MergeRigidNodes(currentRn, rn);
                }
            });
        });

        // this.DebugPrintHierarchy(1, ...this._designHierarchyRoot.children!);

        // 5. Remove Empty RNs
        this._rigidNodes = this._rigidNodes.filter(x => x.parts.size > 0);
    }

    private NewRigidNode(suffix?: string): RigidNode {
        const node = new RigidNode((this._nodeNameCounter++).toString() + (suffix ? `_${suffix}` : ''));
        this._rigidNodes.push(node);
        return node;
    }

    private MergeRigidNodes(rnA: RigidNode, rnB: RigidNode) {
        const newRn = this.NewRigidNode('merged');
        const allParts = new Set<string>([...rnA.parts, ...rnB.parts]);
        allParts.forEach(x => this.MovePartToRigidNode(x, newRn));
        return newRn;
    }

    private MovePartToRigidNode(part: string, node: RigidNode) {
        if (part.length < 1)
            return;
        
        const original = this._partToNodeMap.get(part);
        if (original) {
            if (original === node)
                return;

            original.parts.delete(part);
            this._partToNodeMap.delete(part);
        }

        node.parts.add(part);
        this._partToNodeMap.set(part, node);
    }

    /**
     * Loads this._globalTransforms with the world space transformations of each part instance.
     */
    private LoadGlobalTransforms() {
        const root = this._designHierarchyRoot;
        const partInstances = new Map<string, mirabuf.IPartInstance>(Object.entries(this._assembly.data!.parts!.partInstances!));
        const partDefinitions = this._assembly.data!.parts!.partDefinitions!;

        this._globalTransforms.clear();

        const getTransforms = (node: mirabuf.INode, parent: THREE.Matrix4) => {
            for (const child of node.children!) {
                if (!partInstances.has(child.value!)) {
                    continue;
                }
                const partInstance = partInstances.get(child.value!)!;
                
                if (this._globalTransforms.has(child.value!)) continue;
                const mat = MirabufTransform_ThreeMatrix4(partInstance.transform!)!;

                // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat)}`);

                this._globalTransforms.set(child.value!, mat.premultiply(parent));
                getTransforms(child, mat);
            }
        }

        for (const child of root.children!) {
            const partInstance = partInstances.get(child.value!)!;
            let mat;
            if (!partInstance.transform) {
                const def = partDefinitions[partInstances.get(child.value!)!.partDefinitionReference!];
                if (!def.baseTransform) {
                    mat = new THREE.Matrix4().identity();
                } else {
                    mat = MirabufTransform_ThreeMatrix4(def.baseTransform);
                }
            } else {
                mat = MirabufTransform_ThreeMatrix4(partInstance.transform);
            }

            // console.log(`[${partInstance.info!.name!}] -> ${matToString(mat!)}`);

            this._globalTransforms.set(partInstance.info!.GUID!, mat!);
            getTransforms(child, mat!);
        }
    }

    private FindAncestorialBreak(partA: string, partB: string): [string, string] {
        if (!this._partTreeValues.has(partA) || !this._partTreeValues.has(partB)) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, 'Part not found in tree.']);
            return [partA, partB];
        } else if (partA == partB) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, 'Part A and B are the same.']);
        }

        const ptv = this._partTreeValues;
        let pathA = this._designHierarchyRoot;
        let pathB = this._designHierarchyRoot;
        const valueA = ptv.get(partA)!;
        const valueB = ptv.get(partB)!;

        while (pathA.value! == pathB.value! && pathA.value! != partA && pathB.value! != partB) {
            const ancestorIndexA = this.BinarySearchIndex(valueA, pathA.children!);
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!;
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)];

            const ancestorIndexB = this.BinarySearchIndex(valueB, pathB.children!);
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!;
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)];
        }

        if (pathA.value! == partA && pathA.value! == pathB.value!) {
            const ancestorIndexB = this.BinarySearchIndex(valueB, pathB.children!);
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!;
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)];
        } else if (pathB.value! == partB && pathA.value! == pathB.value!) {
            const ancestorIndexA = this.BinarySearchIndex(valueA, pathA.children!);
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!;
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)];
        }

        return [pathA.value!, pathB.value!];
    }

    private BinarySearchIndex(target: number, children: mirabuf.INode[]): number {
        let l = 0;
        let h = children.length;

        while (h - l > 1) {
            const i = Math.floor((h + l) / 2.0);
            const iVal = this._partTreeValues.get(children[i].value!)!;
            if (iVal > target) {
                h = i;
            } else if (iVal < target) {
                l = i + 1;
            } else {
                return i;
            }
        }

        return Math.floor((h + l) / 2.0);
    }

    private BinarySearchDesignTree(target: string): mirabuf.INode | null {
        let node = this._designHierarchyRoot;
        const targetValue = this._partTreeValues.get(target)!;

        while (node.value != target && node.children) {
            const i = this.BinarySearchIndex(targetValue, node.children!);
            const iValue = this._partTreeValues.get(node.children![i].value!)!;
            node = node.children![i + (iValue < targetValue ? 1 : 0)];
        }

        return node.value! == target ? node : null;
    }

    private GenerateTreeValues() {
        let nextValue = 0;
        const partTreeValues = new Map<string, number>();

        const recursive = (partNode: mirabuf.INode) => {
            partNode.children = partNode.children?.filter(x => x.value != null);
            partNode.children?.forEach(x => recursive(x));
            partTreeValues.set(partNode.value!, nextValue++);
        }

        this._designHierarchyRoot = new mirabuf.Node();
        this._designHierarchyRoot.value = "Importer Generated Root";
        this._designHierarchyRoot.children = [];
        this._designHierarchyRoot.children.push(...this._assembly.designHierarchy!.nodes!);

        recursive(this._designHierarchyRoot);
        this._partTreeValues = partTreeValues;
    }

    private DebugPrintHierarchy(level: number = 1, ...nodes: mirabuf.INode[]) {
        if (level <= 1)
            console.debug('    .... RN DEBUG ....');
        for (const node of nodes) {
            console.debug(`${'-'.repeat(level)} ${this._assembly.data!.parts!.partInstances![node.value!]!.info!.name!} [${this._partToNodeMap.get(node.value!)?.name ?? '---'}]`);
            if (node.children)
                this.DebugPrintHierarchy(level + 1, ...node.children);
        }

        if (level <= 1)
            console.debug('    .... END ....');
    }
}

/**
 * Collection of mirabuf parts that are bound together
 */
class RigidNode {
    public name: string;
    public parts: Set<string> = new Set();

    public constructor(name: string) {
        this.name = name;
    }
}

export class RigidNodeReadOnly {
    private _original: RigidNode;
    
    public get name(): string {
        return this._original.name;
    }

    public get parts(): ReadonlySet<string> {
        return this._original.parts;
    }

    public constructor(original: RigidNode) {
        this._original = original;
    }
}

export default MirabufParser;
