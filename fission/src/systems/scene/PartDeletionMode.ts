import type * as THREE from "three"
import EventSystem from "@/systems/EventSystem.ts"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import PartPickingMode, { type HighlightMap, type PartPick, type PartSelection } from "./PartPickingMode"

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

        this.pending.addPart(pick.guid, {
            guid: pick.guid,
            name: this.getName(pick.guid),
            highlight: { instanceId: pick.instanceId, mesh: pick.object as THREE.BatchedMesh },
        })
    }
}

export default PartDeletionMode
