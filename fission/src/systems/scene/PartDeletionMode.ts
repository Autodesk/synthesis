import type * as THREE from "three"
import { GROUNDED_JOINT_ID } from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import PartPickingMode, { type HighlightMap, type PartPick, type PartSelection } from "./PartPickingMode"

/** Finds guid's node in the design hierarchy along with its parent and children (link references). */
function findHierarchyContext(
    container: mirabuf.IGraphContainer | null | undefined,
    guid: string
): { parentGuid?: string; childGuids: string[] } | undefined {
    function search(
        children: mirabuf.INode[] | undefined | null,
        parentGuid?: string
    ): { parentGuid?: string; childGuids: string[] } | undefined {
        if (!children) return undefined
        for (const node of children) {
            if (node.value === guid) {
                return { parentGuid, childGuids: node.children?.map(c => c.value!).filter(Boolean) ?? [] }
            }
            const found = search(node.children, node.value ?? parentGuid)
            if (found) return found
        }
        return undefined
    }

    return search(container?.nodes, undefined)
}

export interface PartDeletionSelection extends PartSelection {
    name: string
}

// const PENDING_DELETE_HIGHLIGHT_COLOR = new THREE.Color(1, 0.15, 0.15)

/** Interaction mode: click a part to mark it for deletion; click again (or Undo) to unmark. */
class PartDeletionMode extends PartPickingMode<PartDeletionSelection> {
    public constructor() {
        super(
            (isSelected, highlight) => {
                highlight.mesh.setVisibleAt(highlight.instanceId, !isSelected)
            },
            parts => EventSystem.dispatch("PartDeletionSelectionChanged", { parts })
        )
    }

    public get pendingDeletions(): HighlightMap<PartDeletionSelection> {
        return this.pending
    }

    protected override handlePick(pick: PartPick): void {
        if (this._object == null) return
        const rootPartGuid = this.getGroundedRootPartGuid(this._object)
        if (rootPartGuid === pick.guid) {
            globalAddToast("warning", "Delete Parts", "Can't delete the assembly's grounded/root part.")
            return
        }

        this.logSelection(pick.guid)

        this.pending.addPart(pick.guid, {
            guid: pick.guid,
            name: this.getName(pick.guid),
            highlight: { instanceId: pick.instanceId, mesh: pick.object as THREE.BatchedMesh },
        })
    }

    /** Logs the selected part's name, joints (mates) referencing it, and design-hierarchy links, in real time. */
    private logSelection(guid: string): void {
        const assembly = this._object?.mirabufInstance.parser.assembly
        const partInstances = assembly?.data?.parts?.partInstances
        const jointInstances = assembly?.data?.joints?.jointInstances

        const mates = Object.entries(jointInstances ?? {})
            .filter(([key, inst]) => key !== GROUNDED_JOINT_ID && (inst.parentPart === guid || inst.childPart === guid))
            .map(([key, inst]) => ({
                key,
                name: inst.info?.name,
                parentPart: inst.parentPart,
                parentPartName: partInstances?.[inst.parentPart!]?.info?.name,
                childPart: inst.childPart,
                childPartName: partInstances?.[inst.childPart!]?.info?.name,
            }))

        const hierarchy = findHierarchyContext(assembly?.designHierarchy, guid)

        const info = {
            guid,
            name: this.getName(guid),
            mates,
            hierarchy: {
                parentGuid: hierarchy?.parentGuid,
                parentName: hierarchy?.parentGuid ? partInstances?.[hierarchy.parentGuid]?.info?.name : undefined,
                childGuids: hierarchy?.childGuids ?? [],
                childNames: hierarchy?.childGuids?.map(g => partInstances?.[g]?.info?.name) ?? [],
            },
        }

        // Logged as a JSON string (not the raw object) to avoid devtools truncating long arrays/objects in the console.
        console.log(`[PartDeletionMode] selected part:\n${JSON.stringify(info, null, 2)}`)
    }
}

export default PartDeletionMode
