import { mirabuf } from "../proto/mirabuf";

export enum ParseErrorSeverity {
    Unimportable = 1,
    LikelyIssues = 2,
    ProbablyOkay = 3,
    JustAWarning = 4
}

class MirabufParser {

    private _assembly: mirabuf.Assembly;
    private _errors: Array<[severity: ParseErrorSeverity, message: string]>;

    protected _partTreeValues: Map<string, number> = new Map();
    private _designHierarchyRoot: mirabuf.INode = new mirabuf.Node();

    public constructor(assembly: mirabuf.Assembly) {
        this._assembly = assembly;
        this._errors = new Array<[ParseErrorSeverity, string]>();

        this.GenerateTreeValues();
    }

    private GenerateTreeValues() {
        let nextValue = 0;
        const partTreeValues = new Map<string, number>();

        function recursive(partNode: mirabuf.INode) {
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

export default MirabufParser;