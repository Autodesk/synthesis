import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"

function collectSubtreeGuids(node: mirabuf.INode, acc: Set<string>): void {
    if (node.value) acc.add(node.value)
    node.children?.forEach(child => collectSubtreeGuids(child, acc))
}

/** Removes guid's node (and its descendants) from the design hierarchy, wherever it lives. Returns the removed GUIDs. */
function removeFromDesignHierarchy(designHierarchy: mirabuf.IGraphContainer, guid: string): Set<string> {
    const removed = new Set<string>()

    function removeFromChildren(children: mirabuf.INode[] | undefined | null): boolean {
        if (!children) return false

        const index = children.findIndex(n => n.value === guid)
        if (index !== -1) {
            collectSubtreeGuids(children[index], removed)
            children.splice(index, 1)
            return true
        }

        return children.some(child => removeFromChildren(child.children))
    }

    removeFromChildren(designHierarchy.nodes)
    return removed
}

/** Mutates assembly in place: removes each part's subtree and prunes joints/rigid groups referencing it. */
export function applyPartDeletions(assembly: mirabuf.Assembly, partGuids: string[]): void {
    const designHierarchy = assembly.designHierarchy
    if (!designHierarchy) throw new Error("Assembly has no design hierarchy")

    const removedGuids = new Set<string>()
    for (const guid of partGuids) {
        for (const removed of removeFromDesignHierarchy(designHierarchy, guid)) removedGuids.add(removed)
    }

    if (removedGuids.size === 0) return

    const partInstances = assembly.data?.parts?.partInstances
    if (partInstances) {
        for (const guid of removedGuids) delete partInstances[guid]
    }

    const joints = assembly.data?.joints
    if (joints?.jointInstances) {
        for (const [key, inst] of Object.entries(joints.jointInstances)) {
            if (key === GROUNDED_JOINT_ID) continue
            if (removedGuids.has(inst.parentPart!) || removedGuids.has(inst.childPart!)) {
                delete joints.jointInstances[key]
            }
        }
    }

    if (joints?.rigidGroups) {
        for (let i = joints.rigidGroups.length - 1; i >= 0; i--) {
            const group = joints.rigidGroups[i]
            if (!group.occurrences) continue

            group.occurrences = group.occurrences.filter(guid => !removedGuids.has(guid))
            if (group.occurrences.length < 2) joints.rigidGroups.splice(i, 1)
        }
    }
}
