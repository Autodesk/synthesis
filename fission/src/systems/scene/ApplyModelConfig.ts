import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { applyPartDeletions } from "@/mirabuf/PartDeletionBuilder"
import { applyWheelAssignments } from "@/mirabuf/WheelJointBuilder"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import World from "../World"

/**
 * Applies pending wheel assignments and pending part deletions for every affected scene object in a
 * single pass, all within one teardown and build.
 */
export async function applyModelConfigChanges(): Promise<void> {
    const wheelMode = World.wheelAssignmentMode
    const deleteMode = World.partDeletionMode

    const assignments = wheelMode.pendingList.map(selection => selection.assignment)
    const deletions = deleteMode.pendingList.map(selection => selection.guid)
    if (assignments.length === 0 && deletions.length === 0) return

    wheelMode.clearHover()
    deleteMode.clearHover()

    const sceneObject = World.wheelAssignmentMode.sceneObject
    if (sceneObject == null || World.partDeletionMode.sceneObject !== sceneObject) {
        console.warn(
            "Mismatch in part handlers",
            World.wheelAssignmentMode.sceneObject,
            World.partDeletionMode.sceneObject
        )
        return
    }
    const sceneId = sceneObject.id
    if (!sceneObject) return

    const assembly = sceneObject.mirabufInstance.parser.assembly

    if (assignments) applyWheelAssignments(assembly, assignments)
    if (deletions) applyPartDeletions(assembly, deletions)

    World.sceneRenderer.removeSceneObject(sceneId)

    const rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
    if (!rebuilt) {
        globalAddToast("error", "Model Config", "Failed to rebuild assembly.")
        return
    }
    World.sceneRenderer.registerSceneObject(rebuilt, sceneId)

    wheelMode.isMismatched(assignments, rebuilt)
    wheelMode.finishApply()
    deleteMode.finishApply()

    globalAddToast("success", "Model Config", "Applied the pending changes and rebuilt the affected assembly.")
}
