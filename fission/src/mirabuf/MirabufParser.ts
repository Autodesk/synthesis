import { mirabuf } from "../proto/mirabuf";
import Queue from "../util/data/Queue";

class MirabufParser {
    public static ParseRigidNodes(assembly: mirabuf.Assembly) {

    }

    public static GeneratePartPaths(assembly: mirabuf.Assembly): Map<string, Queue<string>> {
        const graph = assembly.designHierarchy!;
        const pathMap = new Map<string, Queue<string>>();

        
    }

    private static GeneratePartParts(node: mirabuf.INode, currentQueue: Queue<string>) {

    }
}

export default MirabufParser;