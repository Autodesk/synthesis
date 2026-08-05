import * as THREE from "three"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import PartPickingMode, { type HighlightMap, type PartPick, type PartSelection } from "./PartPickingMode"

export interface PartDeletionSelection extends PartSelection {
    name: string
}

/** Tint for parts pending deletion. */
const PENDING_DELETE_HIGHLIGHT_COLOR = new THREE.Color(1, 0.15, 0.15)

/** Interaction mode: click a part to mark it for deletion; click again (or Undo) to unmark. */
class PartDeletionMode extends PartPickingMode<PartDeletionSelection> {
    public constructor() {
        super(PENDING_DELETE_HIGHLIGHT_COLOR, parts => EventSystem.dispatch("PartDeletionSelectionChanged", { parts }))
    }

    /** Alias kept for existing call sites (DeleteParts.tsx). */
    public get pendingDeletions(): HighlightMap<PartDeletionSelection> {
        return this.pending
    }

    protected handlePick(pick: PartPick): void {
        // Grounded/root part can't be removed -- nothing left to attach the assembly to.
        const rootPartGuid = this.getGroundedRootPartGuid(pick.sceneObject)
        if (rootPartGuid === pick.guid) {
            globalAddToast("warning", "Delete Parts", "Can't delete the assembly's grounded/root part.")
            return
        }

        const partInstances = pick.sceneObject.mirabufInstance.parser.assembly.data?.parts?.partInstances
        const name = partInstances?.[pick.guid]?.info?.name ?? pick.guid

        this.pending.addPart(pick.guid, {
            sceneId: pick.sceneObject.id,
            guid: pick.guid,
            name,
            highlight: { instanceId: pick.instanceId, mesh: pick.object as THREE.BatchedMesh },
        })
    }
}

export default PartDeletionMode
