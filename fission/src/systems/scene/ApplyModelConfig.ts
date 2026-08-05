import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
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

    const assignmentsBySceneId = wheelMode.collectPendingBySceneId(selection => selection.assignment)
    const deletionsBySceneId = deleteMode.collectPendingBySceneId(selection => selection.guid)
    if (assignmentsBySceneId.size === 0 && deletionsBySceneId.size === 0) return

    wheelMode.clearHover()
    deleteMode.clearHover()

    const sceneIds = new Set([...assignmentsBySceneId.keys(), ...deletionsBySceneId.keys()])
    const rebuiltBySceneId = new Map<number, MirabufSceneObject>()

    let rebuildFailed = false
    for (const sceneId of sceneIds) {
        const sceneObject = World.sceneRenderer.sceneObjects.get(sceneId) as MirabufSceneObject | undefined
        if (!sceneObject) continue

        const assembly = sceneObject.mirabufInstance.parser.assembly

        const assignments = assignmentsBySceneId.get(sceneId)
        if (assignments) applyWheelAssignments(assembly, assignments)

        const deletions = deletionsBySceneId.get(sceneId)
        if (deletions) applyPartDeletions(assembly, deletions)

        World.sceneRenderer.removeSceneObject(sceneId)

        const rebuilt = await createMirabuf(assembly.info!.GUID!, assembly)
        if (!rebuilt) {
            rebuildFailed = true
            continue
        }
        World.sceneRenderer.registerSceneObject(rebuilt, sceneId)
        rebuiltBySceneId.set(sceneId, rebuilt)
    }

    wheelMode.warnIfMismatched(assignmentsBySceneId, rebuiltBySceneId)
    wheelMode.finishApply()
    deleteMode.finishApply()

    if (rebuildFailed) {
        globalAddToast("error", "Model Config", "Failed to rebuild one or more assemblies after applying changes.")
    } else {
        globalAddToast("success", "Model Config", "Applied the pending changes and rebuilt the affected assembly.")
    }
}
