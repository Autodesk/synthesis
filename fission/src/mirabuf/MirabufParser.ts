import { mirabuf } from "../proto/mirabuf";

export enum ParseErrorSeverity {
    Unimportable = 1,
    LikelyIssues = 2,
    ProbablyOkay = 3,
    JustAWarning = 4
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

    public get partTreeValues() {
        return this._partTreeValues;
    }

    public get designHierarchyRoot() {
        return this._designHierarchyRoot;
    }

    public get partToNodeMap() {
        return this._partToNodeMap;
    }

    public get rigidNodes() {
        return this._rigidNodes;
    }

    public constructor(assembly: mirabuf.Assembly) {
        this._assembly = assembly;
        this._errors = new Array<ParseError>();

        this.GenerateTreeValues();

        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const that = this;

        const traverseJointParts = (nodes: mirabuf.INode[], rn: RigidNode) => {
            nodes.forEach(x => {
                if (x.children) {
                    traverseJointParts(x.children, rn);
                }
                that.MovePartToRigidNode(x.value!, rn);
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
                    traverseJointParts(jInst.parts.nodes, parentRN);
            }
        });

        // 2: Grounded joint
        const gInst = assembly.data!.joints!.jointInstances![GROUNDED_JOINT_ID];
        const gNode = this.NewRigidNode();
        
        traverseJointParts(gInst.parts!.nodes!, gNode);
        
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

        // 4: Bandage via rigidgroups
        assembly.data!.joints!.rigidGroups!.forEach(rg => {
            const rn = this.NewRigidNode(rg.name!);
            rg.occurrences!.forEach(y => this.MovePartToRigidNode(y, rn));
        });

        // 5. Remove Empty RNs
        this._rigidNodes = this._rigidNodes.filter(x => x.parts.size > 0);
    }

    private NewRigidNode(suffix?: string): RigidNode {
        const node = new RigidNode((this._nodeNameCounter++).toString() + (suffix ? `_${suffix}` : ''));
        this._rigidNodes.push(node);
        return node;
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

    private FindAncestorialBreak(partA: string, partB: string): [string, string] {
        if (!this._partTreeValues.has(partA) || !this._partTreeValues.has(partB)) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, 'Part not found in tree.']);
            return [partA, partB];
        } else if (partA == partB) {
            this._errors.push([ParseErrorSeverity.LikelyIssues, 'Part A and B are the same.']);
        }

        const ptv = this._partTreeValues;

        const binarySearch = (target: number, children: mirabuf.INode[]): number => {
            let l = 0;
            let h = children.length;

            while (h - l > 1) {
                const i = Math.floor((h + l) / 2.0);
                const iVal = ptv.get(children[i].value!)!;
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

        let pathA = this._designHierarchyRoot;
        let pathB = this._designHierarchyRoot;
        const valueA = ptv.get(partA)!;
        const valueB = ptv.get(partB)!;

        while (pathA.value! == pathB.value! && pathA.value! != partA && pathB.value! != partB) {
            const ancestorIndexA = binarySearch(valueA, pathA.children!);
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!;
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)];

            const ancestorIndexB = binarySearch(valueB, pathB.children!);
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!;
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)];
        }

        if (pathA.value! == partA && pathA.value! == pathB.value!) {
            const ancestorIndexB = binarySearch(valueB, pathB.children!);
            const ancestorValueB = ptv.get(pathB.children![ancestorIndexB].value!)!;
            pathB = pathB.children![ancestorIndexB + (ancestorValueB < valueB ? 1 : 0)];
        } else if (pathB.value! == partB && pathA.value! == pathB.value!) {
            const ancestorIndexA = binarySearch(valueA, pathA.children!);
            const ancestorValueA = ptv.get(pathA.children![ancestorIndexA].value!)!;
            pathA = pathA.children![ancestorIndexA + (ancestorValueA < valueA ? 1 : 0)];
        }

        return [pathA.value!, pathB.value!];
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
}

/**
 * Collection of mirabuf parts that are bound together
 */
export class RigidNode {
    public name: string;
    public parts: Set<string> = new Set();

    public constructor(name: string) {
        this.name = name;
    }
}

export default MirabufParser;
